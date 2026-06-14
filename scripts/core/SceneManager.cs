using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using pokemonGodot.Scripts.Gameplay.Levels;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Core
{
	public partial class SceneManager : Node
	{

		public static SceneManager Instance { get; private set; }
		public static bool IsChanging { get; private set; }

		[ExportCategory("SceneManager Vars")]
		[Export] public ColorRect FadeRect;
		[Export] public Level CurrentLevel;

		[Export]
		public Array<Level> AllLevels;
		
		
		
		


		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Instance = this;
			IsChanging = false;

			Logger.Info("Loading SceneManager...");

			SceneManager.ChangeLevel();
		}

		public static async void ChangeLevel(LevelName levelName = LevelName.small_town, int trigger = 0, bool spawn = false)
		{
			while (IsChanging)
			{
				await Instance.ToSignal(Instance.GetTree(), "process_frame");
			}

			IsChanging = true;

			await Instance.GetLevel(levelName);

			if (spawn)
			{
				Instance.Spawn();	
			}
			else
			{
				Instance.Switch(trigger);
				
			}
			await Instance.FadeIn();

			IsChanging = false;
		}

		public async Task GetLevel(LevelName levelName)
		{
			if (CurrentLevel != null)
			{
				await Instance.FadeOut();
				GameManager.GetGameViewPort().RemoveChild(CurrentLevel);

			}

			CurrentLevel = AllLevels.FirstOrDefault(level => level.LevelName == levelName);

			if (CurrentLevel != null)
			{
				GameManager.GetGameViewPort().AddChild(CurrentLevel);
			}
			else
			{
				CurrentLevel = GD.Load<PackedScene>($"res://scenes/levels/{levelName}.tscn").Instantiate<Level>();
				AllLevels.Add(CurrentLevel);
				GameManager.GetGameViewPort().AddChild(CurrentLevel);
			}
		}

		public void Switch(int trigger)
		{
			var sceneTriggers = CurrentLevel.GetTree().GetNodesInGroup(LevelGroups.SCENETRIGGERS.ToString());

			if (sceneTriggers.Count <= 0)
			{
				throw new Exception("Missing Scene Trigger");
			}
			if (sceneTriggers.FirstOrDefault(st => ((SceneTrigger)st).CurrentLevelTrigger == trigger) is not SceneTrigger sceneTrigger)
			{
				throw new Exception($"Missing Scene trigger for trigger {trigger}");
			}

			GameManager.GetPlayer().Position = sceneTrigger.Position + sceneTrigger.EntryDirection * Globals.Instance.GRID_SIZE;


		}

		public void Spawn()
		{
			 var spawnPoints = CurrentLevel.GetTree().GetNodesInGroup(LevelGroups.SPAWNPOINTS.ToString());

			 if (spawnPoints.Count <= 0)
			{
				throw new Exception("Missing Spawn Point");
			}
		
			 var spawnPoint = (SpawnPoint)spawnPoints[0];
			 var player = GD.Load<PackedScene>($"res://scenes/characters/player.tscn").Instantiate<Player>();


			GameManager.AddPlayer(player);
			GameManager.GetPlayer().Position = spawnPoint.Position;
			
		}

		public async Task FadeOut()
		{
			Tween tween = CreateTween();
			tween.TweenProperty(FadeRect, "color:a", 1.0, 0.25);
			await ToSignal(tween, "finished");
		}

		public async Task FadeIn()
		{
			Tween tween = CreateTween();
			tween.TweenProperty(FadeRect, "color:a", 0.0, 0.25);
			await ToSignal(tween, "finished");
		}

	}
}
