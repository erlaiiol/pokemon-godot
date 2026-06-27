using Godot;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using pokemonGodot.Scripts.Utilities;

namespace pokemonGodot.Scripts.Gameplay.States
{
	public partial class NpcRoamState : State
	{
		[ExportCategory("State Vars")]
		[Export] public NpcInput NpcInput;
		[Export] public CharacterMovement CharacterMovement;

		private double _timer = 0;

		// Tutoriel : tableau de directions. AVANT c'était Vector2[] { Vector2.Up, ... }
		// APRÈS : on utilise notre enum — le compilateur garantit qu'on ne peut pas mettre
		// une valeur invalide, et le switch dans ToVector2() fait la conversion au moment voulu
		private static readonly Direction[] AllDirections =
			[Direction.Up, Direction.Down, Direction.Left, Direction.Right];

		public override void _Process(double delta)
		{
			if (CharacterMovement.IsMoving()) return;

			_timer += delta;

			switch (NpcInput.Config.NpcMovementType)
			{
				case NpcMovementType.Wander:     HandleWander();     break;
				case NpcMovementType.Patrol:     HandlePatrol();     break;
				case NpcMovementType.LookAround: HandleLookAround(); break;
			}
		}

		private void HandleWander()
		{
			if (_timer < NpcInput.Config.WanderMoveInterval) return;
			_timer = 0;

			int randomIndex = Globals.GetRandomNumberGenerator().RandiRange(0, AllDirections.Length - 1);
			Direction randomDir = AllDirections[randomIndex];

			Vector2 targetPos = CharacterMovement.Character.GlobalPosition
								+ randomDir.ToVector2() * Globals.Instance.GRID_SIZE;

			if (targetPos.DistanceTo(NpcInput.Config.WanderOrigin) <= NpcInput.Config.WanderRadius)
			{
				bool directionChanged = NpcInput.Direction != randomDir;
				NpcInput.Direction = randomDir;

				// Si le NPC change de direction, il tourne d'abord (comme dans Pokémon)
				// Si même direction, il marche directement sans tour intermédiaire
				if (directionChanged)
					NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
				else
					NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
			}
		}

		private void HandlePatrol()
		{
			if (_timer < NpcInput.Config.PatrolMoveInterval) return;
			if (NpcInput.Config.PatrolPoints == null || NpcInput.Config.PatrolPoints.Count == 0) return;
			_timer = 0;

			Vector2 target = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
			Vector2 current = CharacterMovement.Character.GlobalPosition;

			Direction neededDirection = DirectionToward(current, target);
			bool directionChanged = NpcInput.Direction != neededDirection;
			NpcInput.Direction = neededDirection;

			if (directionChanged)
				NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
			else
				NpcInput.EmitSignal(CharacterInput.SignalName.Walk);

			if (current.DistanceTo(target) < Globals.Instance.GRID_SIZE)
			{
				NpcInput.Config.PatrolIndex =
					(NpcInput.Config.PatrolIndex + 1) % NpcInput.Config.PatrolPoints.Count;
			}
		}

		private void HandleLookAround()
		{
			if (_timer < NpcInput.Config.LookAroundInterval) return;
			_timer = 0;

			int randomIndex = Globals.GetRandomNumberGenerator().RandiRange(0, AllDirections.Length - 1);
			NpcInput.Direction = AllDirections[randomIndex];

			// LookAround : on tourne sur place, on n'émet PAS Walk mais Turn
			NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
		}

		// Privée car seul NpcRoamState en a besoin.
		// Principe YAGNI : si un autre système en a besoin un jour, on extrait à ce moment-là.
		private static Direction DirectionToward(Vector2 from, Vector2 to)
		{
			Vector2 diff = to - from;
			if (Mathf.Abs(diff.X) >= Mathf.Abs(diff.Y))
				return diff.X > 0 ? Direction.Right : Direction.Left;
			return diff.Y > 0 ? Direction.Down : Direction.Up;
		}
	}
}
