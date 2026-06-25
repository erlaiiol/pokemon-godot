using Godot;
using System;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay.Levels
{
	public partial class SceneTrigger : Area2D
	{

		[ExportCategory("Target Scene Vars")] 
		
		[Export] public LevelName TargetLevelName;
		
		[Export] public int TargetLevelTrigger = 0;

		[ExportCategory("Current Scene Vars")] 
		
		[Export] public int CurrentLevelTrigger = 0;

		[Export] public Vector2 EntryDirection;

		[Export] public bool Locked = false;
		
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			BodyEntered += OnBodyEntered;
		}

		public void OnBodyEntered(Node2D body)
		{
			if (body.Name != "Player") return;
			if (SceneManager.IsChanging) return;

			if (Locked)
			{
				Logger.Info("Uh oh, the level is locked");
				return;
			}

			SceneManager.ChangeLevel(levelName: TargetLevelName, trigger: TargetLevelTrigger);
		}
		public override void _EnterTree()
		{
			AddToGroup(LevelGroups.SCENETRIGGERS.ToString());
		}

		public override void _ExitTree()
		{
			RemoveFromGroup(LevelGroups.SCENETRIGGERS.ToString());
		}
	}

}
