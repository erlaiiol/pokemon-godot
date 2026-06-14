using Godot;
using System;
using pokemonGodot.Scripts.Core;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{

	public partial class CharacterMovement : Node
	{

		[Signal]
		public delegate void AnimationEventHandler(string animationType);
		
		[ExportCategory("Nodes")]
		
		[Export] public Node2D Character;

		[Export] public CharacterInput CharacterInput;
		
		[ExportCategory("Movement")]
		
		[Export] public Vector2 TargetPosition = Vector2.Down;

		[Export] public bool IsWalking = false;
		
		[Export] public bool CollisionDetected = false;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			CharacterInput.Walk += StartWalking;
			CharacterInput.Turn += Turn;
			
			Logger.Info("Loading Character Movement...");
		}

		
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			Walk(delta);
		}
		
		public bool IsMoving()
		{
			return IsWalking;
		}

		public bool IsColliding()
		{
			return CollisionDetected;
		}

		private bool IsTargetOccupied(Vector2 targetPosition)
		{
			var spaceState = GetViewport().GetWorld2D().DirectSpaceState;

			Vector2 adjustedTargetPosition = TargetPosition;
			adjustedTargetPosition.Y += 8;
			adjustedTargetPosition.X += 8;

			var query = new PhysicsPointQueryParameters2D
			{
				Position = adjustedTargetPosition,
				CollisionMask = 1,
				CollideWithAreas = true
			};

			var result = spaceState.IntersectPoint(query);

			if (result.Count > 0)
			{
				foreach (var collision in result)
				{
					var collider = (Node)(GodotObject)collision["collider"];
					var colliderType = collider.GetType().Name;

					return colliderType switch
					{
						"TileMapLayer" => true,
						"SceneTrigger" => false,
						_ => true
					};
				}

			}
			
			return false;
		}
		
		
		public void StartWalking()
		{
			
			TargetPosition = Character.Position + CharacterInput.Direction * Globals.Instance.GRID_SIZE;
			
			if (!IsMoving() && !IsTargetOccupied(TargetPosition))
			{

				EmitSignal(SignalName.Animation, "walk");

				
				Logger.Info("Moving character to " + TargetPosition + "from " + Character.Position);
				IsWalking = true;
			}
			else
			{
				EmitSignal(SignalName.Animation, "idle");
			}
		}


		public void Walk(double delta)
		{
			if (IsWalking)
			{
				Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.Instance.GRID_SIZE * 4);

				if (Character.Position.DistanceTo(TargetPosition) <= 1f)
				{
					StopWalking();
				}
			}
			else
			{
				
			}
		}

		public void StopWalking()
		{
			IsWalking = false;
			SnapPositionToGrid();
			if (!Modules.IsActionPressed())
				EmitSignal(SignalName.Animation, "idle");
		}

		public void Turn()
		{
			EmitSignal(SignalName.Animation, "turn");
		}

		public void SnapPositionToGrid()
		{
			Character.Position = new Vector2(
				Mathf.Round(Character.Position.X / Globals.Instance.GRID_SIZE) * Globals.Instance.GRID_SIZE,
				Mathf.Round(Character.Position.Y / Globals.Instance.GRID_SIZE) * Globals.Instance.GRID_SIZE
				);
		}
		
		
	}
}
