using Godot;
using Godot.Collections;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using pokemonGodot.Scripts.Gameplay.Levels;
using pokemonGodot.Scripts.Utilities;

namespace pokemonGodot.Scripts.Gameplay.States
{
	public partial class NpcRoamState : State
	{
		[ExportCategory("State Vars")]
		[Export] public NpcInput NpcInput;
		[Export] public CharacterMovement CharacterMovement;

		private double _timer = 0;

		private static readonly Direction[] AllDirections =
			[Direction.Up, Direction.Down, Direction.Left, Direction.Right];

		// État interne du mode Patrol : chemin A* courant et position dans ce chemin.
		// Réinitialisés dans EnterState() pour recalculer depuis la position actuelle
		// après toute interruption (message du joueur, changement de scène, etc.).
		private Array<Vector2I> _patrolPath = [];
		private int _patrolPathIndex = 0;

		// Appelé par StateMachine.ChangeState() à chaque entrée dans cet état.
		// Garantit que le Patrol repart toujours d'un chemin frais depuis la position snappée.
		public override void EnterState()
		{
			base.EnterState();
			_patrolPath = [];
			_patrolPathIndex = 0;
		}

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

				if (directionChanged)
					NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
				else
					NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
			}
		}

		private void HandlePatrol()
		{
			if (NpcInput.Config.PatrolPoints == null || NpcInput.Config.PatrolPoints.Count == 0) return;

			// Chemin épuisé ou pas encore calculé : en calculer un vers le prochain patrol point.
			if (_patrolPath.Count == 0)
			{
				Vector2 target = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
				_patrolPath = ComputePatrolPath(CharacterMovement.Character.GlobalPosition, target);
				_patrolPathIndex = 1; // Index 0 = position actuelle, déjà occupée.

				if (_patrolPath.Count <= 1)
				{
					// Déjà sur la cible ou cible inatteignable : passer au point suivant.
					AdvancePatrolIndex();
					_patrolPath = [];
					return;
				}
			}

			if (_timer < NpcInput.Config.PatrolMoveInterval) return;

			Vector2I currentCell = WorldToCell(CharacterMovement.Character.GlobalPosition);
			Vector2I nextCell = _patrolPath[_patrolPathIndex];

			// Cas résiduel : déjà sur la prochaine case attendue.
			if (currentCell == nextCell)
			{
				_patrolPathIndex++;
				if (_patrolPathIndex >= _patrolPath.Count) { AdvancePatrolIndex(); _patrolPath = []; }
				return;
			}

			Direction neededDirection = CellDirection(currentCell, nextCell);
			bool directionChanged = NpcInput.Direction != neededDirection;
			NpcInput.Direction = neededDirection;

			// Turn d'abord : on reset le timer pour laisser le temps à l'animation.
			if (directionChanged)
			{
				_timer = 0;
				NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
				return;
			}

			NpcInput.EmitSignal(CharacterInput.SignalName.Walk);

			if (!CharacterMovement.IsMoving())
			{
				// Walk refusé par StartMoving() : case cible occupée.
				// On identifie le bloqueur pour choisir le comportement.
				var (_, collisions) = CharacterMovement.GetTargetColliders(CharacterMovement.TargetPosition);
				foreach (var collision in collisions)
				{
					var blocker = (Node)(GodotObject)collision["collider"];
					if (blocker.GetType().Name == "Player")
					{
						// Joueur dynamique : attendre qu'il libère la case.
						// On impose un délai minimum de 0.5s avant la prochaine tentative,
						// quel que soit PatrolMoveInterval, pour éviter de boucler chaque frame.
						_timer = NpcInput.Config.PatrolMoveInterval - 0.5f;
						return;
					}
				}
				// Obstacle statique inattendu (autre NPC arrivé entre-temps, etc.) :
				// le chemin A* est obsolète, on le recalcule depuis la position actuelle.
				_patrolPath = [];
				return;
			}

			// Walk accepté : on avance dans le chemin et on réinitialise le timer.
			_timer = 0;
			_patrolPathIndex++;

			if (_patrolPathIndex >= _patrolPath.Count)
			{
				AdvancePatrolIndex();
				_patrolPath = [];
			}
		}

		private void HandleLookAround()
		{
			if (_timer < NpcInput.Config.LookAroundInterval) return;
			_timer = 0;

			int randomIndex = Globals.GetRandomNumberGenerator().RandiRange(0, AllDirections.Length - 1);
			NpcInput.Direction = AllDirections[randomIndex];

			NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
		}

		private void AdvancePatrolIndex()
		{
			NpcInput.Config.PatrolIndex =
				(NpcInput.Config.PatrolIndex + 1) % NpcInput.Config.PatrolPoints.Count;
		}

		// Calcule le chemin A* entre deux positions monde via la grille du niveau courant.
		// GetIdPath inclut la cellule de départ (index 0) et la cible (dernier index).
		// Retourne un tableau vide si la cible est inatteignable.
		private static Array<Vector2I> ComputePatrolPath(Vector2 from, Vector2 to)
		{
			Level level = SceneManager.GetCurrentLevel();
			if (level?.Grid == null) return [];

			return level.Grid.GetIdPath(WorldToCell(from), WorldToCell(to));
		}

		// Position monde (pixels) → cellule grille.
		// Les positions NPC sont toujours des multiples de GRID_SIZE après StopMoving().
		private static Vector2I WorldToCell(Vector2 worldPos) => new(
			Mathf.FloorToInt(worldPos.X / Globals.Instance.GRID_SIZE),
			Mathf.FloorToInt(worldPos.Y / Globals.Instance.GRID_SIZE)
		);

		// Direction cardinale entre deux cellules adjacentes.
		// A* Manhattan sans diagonale garantit que diff vaut toujours (±1,0) ou (0,±1).
		private static Direction CellDirection(Vector2I from, Vector2I to)
		{
			Vector2I diff = to - from;
			if (diff.X != 0) return diff.X > 0 ? Direction.Right : Direction.Left;
			return diff.Y > 0 ? Direction.Down : Direction.Up;
		}
	}
}
