using Godot;
using System;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using Logger = pokemonGodot.Scripts.Core.Logger;
using Godot.Collections;

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

		[Export] public Vector2 StartPosition;

		[Export] public bool IsJumping = false;

		[Export] public float  JumpHeight = 10f;

		[Export] public float LerpSpeed = 2f;

		[Export] public float Progress = 0f;

		[Export] public ECharacterMovement ECharacterMovement = ECharacterMovement.WALKING;
		
		[Export] public bool CollisionDetected = false;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			CharacterInput.Walk += StartMoving;
			CharacterInput.Turn += Turn;
			
			Logger.Info("Loading Character Movement...");
		}

		
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			Walk(delta);
			Jump(delta);

			if (IsMoving() && !Modules.IsActionJustPressed())
			{
				EmitSignal(SignalName.Animation, "idle");
			}
		}
		
		public bool IsMoving()
		{
			return IsWalking || IsJumping;
		}

		public bool IsColliding()
		{
			return CollisionDetected;
		}

		public (Vector2, Array<Dictionary>) GetTargetColliders(Vector2 targetPosition)
		{
			var spaceState = GetViewport().GetWorld2D().DirectSpaceState;

			var query = new PhysicsPointQueryParameters2D
			{
				Position = targetPosition,
				CollisionMask = 1,
				CollideWithAreas = true
			};

			return (targetPosition, spaceState.IntersectPoint(query));
		}

		private bool IsTargetOccupied(Vector2 targetPosition)
		{


			var (adjustedTargetPosition, result) = GetTargetColliders(targetPosition);
			
			if (result.Count == 0)
			{
				return false;
			}
			else if (result.Count == 1)
			{
				var collider = (Node)(GodotObject)result[0]["collider"];
				var colliderType = collider.GetType().Name;

				return colliderType switch
					{
						"Sign" => true,
						"TileMapLayer" => GetTileMapLayerCollision((TileMapLayer)collider, adjustedTargetPosition),
						"SceneTrigger" => false,
						_ => true
					};
			}
			else
			{
				return true; 
			}

		}

		public bool GetTileMapLayerCollision(TileMapLayer tileMapLayer, Vector2 adjustedTargetPosition)
		{
			Vector2I tileCoordinates = tileMapLayer.LocalToMap(adjustedTargetPosition);
			TileData tileData = tileMapLayer.GetCellTileData(tileCoordinates);

			if (tileData == null)
			{
				return true;
			}

			var ledgeDirection = (string)tileData.GetCustomData("LEDGE");

			if (ledgeDirection == null)
			{
				return true;
			}

			Logger.Info($"Ledge detected with direction {ledgeDirection}");
			
			switch (ledgeDirection)
			{
				case "DOWN":
					if (CharacterInput.Direction == Direction.Down)
						{
							ECharacterMovement = ECharacterMovement.JUMPING;
							return false;
						}
						break;
				case "LEFT":
					if (CharacterInput.Direction == Direction.Left)
						{
							ECharacterMovement = ECharacterMovement.JUMPING;
							return false;
						}
						break;
				case "RIGHT":
					if (CharacterInput.Direction == Direction.Right)
						{
							ECharacterMovement = ECharacterMovement.JUMPING;
							return false;
						}
						break;
				case "UP":
					if (CharacterInput.Direction == Direction.Up)
						{
							ECharacterMovement = ECharacterMovement.JUMPING;
							return false;
						}
						break;
						 
					default:
						return true;

				
			}
			return true;
		}
		
		
		public void StartMoving()
		{
			if (SceneManager.IsChanging) return;

			TargetPosition = Character.Position + CharacterInput.Direction.ToVector2() * Globals.Instance.GRID_SIZE;
			
			if (!IsMoving() && !IsTargetOccupied(TargetPosition) && SceneManager.GetCurrentLevel().ReserveTile(TargetPosition))
			{

				EmitSignal(SignalName.Animation, "walk");

				
				Logger.Info("Moving character to " + TargetPosition + "from " + Character.Position);
				
				if (ECharacterMovement == ECharacterMovement.JUMPING)
				{
					Progress = 0f;
					IsJumping = true;
					StartPosition = Character.Position;
					TargetPosition = Character.Position + CharacterInput.Direction.ToVector2() * (Globals.Instance.GRID_SIZE * 2);
				}
				else
				{
					IsWalking = true;
				}

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
					StopMoving();
				}
			}
		}


		public void Jump(double delta)
		{
			if (IsJumping)
			{
				Progress += LerpSpeed * (float)delta;
				Vector2 position = StartPosition.Lerp(TargetPosition, Progress);
				float parabolicOffset = JumpHeight * (1 - 4 * (Progress - 0.5f) * (Progress - 0.5f));
				position.Y -= parabolicOffset;
				Character.Position = position;
			}

			if (Progress >= 1f)
			{
				StopMoving();
			}	
		}

		public void StopMoving()
		{
			SceneManager.GetCurrentLevel().ReleaseTile(TargetPosition);
			IsWalking = false;
			IsJumping = false;
			Progress = 0f;

			ECharacterMovement = ECharacterMovement.WALKING;


			SnapPositionToGrid();
			if (!Modules.IsActionPressed())
				EmitSignal(SignalName.Animation, "idle");
		}



		public void PlaceAt(Vector2 worldPosition)
		{
			IsWalking = false;
			Character.GlobalPosition = Globals.SnapToGrid(worldPosition);
			TargetPosition = Character.Position;
			EmitSignal(SignalName.Animation, "idle");
		}

		public void Turn()
		{
			EmitSignal(SignalName.Animation, "turn");
		}

		public void SnapPositionToGrid()
		{
			Character.Position = Globals.SnapToGrid(Character.Position);
		}
		
		
	}
}
