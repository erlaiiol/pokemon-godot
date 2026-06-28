using Godot;
using System;
using pokemonGodot.Scripts.Core.Enums;
using Logger = pokemonGodot.Scripts.Core.Logger;
using System.Collections.Generic;
using pokemonGodot.Scripts.Core;


namespace pokemonGodot.Scripts.Gameplay.Levels
{


	public partial class Level : Node2D
	{

		[ExportCategory("Level Basics")] 
		[Export] public LevelName LevelName;
		[Export(PropertyHint.Range, "0,100")] public int EncounterRate;

		[ExportCategory("Camera Limits")] 
		[Export] public int Top;
		
		[Export] public int Bottom;
		
		[Export] public int Left;
		
		[Export] public int Right;

		private readonly HashSet<Vector2> reservedTiles = [];

		public AStarGrid2D Grid;

		
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Logger.Info($"Loading Level {LevelName}...");	
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (Grid == null && GameManager.GetPlayer() != null)
			{
				SetupGrid();
			}

			if (Grid != null)
			{
				QueueRedraw();
			}
		}


		public void SetupGrid()
		{
		Logger.Info("Setting up A* Grid...");

		Grid = new()
		{
			Region = new Rect2I(0, 0, Right, Bottom),
			CellSize = new Vector2(Globals.Instance.GRID_SIZE, Globals.Instance.GRID_SIZE),
			DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never
		};

		Grid.Update();

		var mapHeight = Bottom / Globals.Instance.GRID_SIZE;
		var mapWidth = Right / Globals.Instance.GRID_SIZE;
		var movement = GameManager.GetPlayer().GetNode<CharacterMovement>("Movement");
		float half = Globals.Instance.GRID_SIZE / 2f;

		for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					Vector2I cell = new(x, y);
					// On query au CENTRE de la tile, pas au coin, pour éviter de détecter
					// les corps physiques des tiles adjacentes sur les frontières communes.
					Vector2 queryPosition = new(x * Globals.Instance.GRID_SIZE + half, y * Globals.Instance.GRID_SIZE + half);

					var (_, collisions) = movement.GetTargetColliders(queryPosition);

					foreach (var collision in collisions)
					{
						var collider = (Node)(GodotObject)collision["collider"];
						var colliderType = collider.GetType().Name;

						if (colliderType is "TallGrass" or "Player" or "SceneTrigger")
							continue;

						if (colliderType == "Npc")
						{
							var npcMovement = ((Npc)collider).NpcInputConfig.NpcMovementType;
							if (npcMovement is NpcMovementType.Patrol or NpcMovementType.Wander)
								continue;
						}

						Grid.SetPointSolid(cell, true);
					}
				}
			}
		}


		public override void _Draw()
		{
			if (Grid == null)
			{
				return;
			}

		var mapHeight = Bottom / Globals.Instance.GRID_SIZE;
		var mapWidth = Right / Globals.Instance.GRID_SIZE;

		for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					Vector2I cell = new(x, y);
					Vector2 worldPosition = new(x * Globals.Instance.GRID_SIZE, y * Globals.Instance.GRID_SIZE);

					var color = Grid.IsPointSolid(cell) ? new Color(1,0,0,0.3f) : new Color(0,1,0,0.3f);
					DrawRect(new Rect2(worldPosition, Grid.CellSize), color, filled:true);
				}
			}
		
		}

		public bool ReserveTile(Vector2 position)
		{
			 if (reservedTiles.Contains(position))
			{
				return false;
			}
			reservedTiles.Add(position);
			return true;
		}

		public bool IsTileFree(Vector2 position)
		{
			return !reservedTiles.Contains(position);
		}

		public void ReleaseTile(Vector2 position)
		{
			reservedTiles.Remove(position);
		}
	}
}
