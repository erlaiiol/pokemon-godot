using Godot;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using pokemonGodot.Scripts.Utilities;
using pokemonGodot.Scripts.Gameplay.Levels;

namespace pokemonGodot.Scripts.Gameplay.States
{
	public partial class PlayerRoamState : State
	{
		[ExportCategory("State Vars")]

		[Export] public PlayerInput PlayerInput;

		[Export] public CharacterMovement CharacterMovement;

		public override void _Ready()
		{
			Signals.Instance.MessageBoxOpen += (value) =>
			{
				if (value)
				{
					StateMachine.ChangeState("Message");
				}
			};
		}

		public override void _Process(double delta)
		{
			GetInputDirection();
			GetInput(delta);
			GetUseInput();
		}

		public void GetInputDirection()
		{
			if (Input.IsActionJustPressed("ui_up"))
				PlayerInput.Direction = Direction.Up;
			else if (Input.IsActionJustPressed("ui_down"))
				PlayerInput.Direction = Direction.Down;
			else if (Input.IsActionJustPressed("ui_left"))
				PlayerInput.Direction = Direction.Left;
			else if (Input.IsActionJustPressed("ui_right"))
				PlayerInput.Direction = Direction.Right;
		}

		public void GetInput(double delta)
		{
			if (CharacterMovement.IsMoving()) return;

			if (Modules.IsActionJustReleased())
			{
				if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
					PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
				else
					PlayerInput.EmitSignal(CharacterInput.SignalName.Turn);

				PlayerInput.HoldTime = 0.0f;
			}

			if (Modules.IsActionPressed())
			{
				PlayerInput.HoldTime += delta;

				if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
					PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
			}
		}

		public void GetUseInput()
		{
			if (Input.IsActionJustReleased("use"))
			{
				Vector2 lookTarget = CharacterMovement.Character.GlobalPosition
					+ PlayerInput.Direction.ToVector2() * Globals.Instance.GRID_SIZE;
				var (_, result) = CharacterMovement.GetTargetColliders(lookTarget);

				foreach (var collision in result)
				{
					var collider = (Node)(GodotObject)collision["collider"];

					switch (collider.GetType().Name)
					{
						case "Sign":
							((Sign)collider).PlayMessage();
							break;
						case "Npc":
							((Npc)collider).PlayMessage(PlayerInput.Direction);
							break;
					}
				}
			}
		}
	}
}
