using Godot;
using System;
using pokemonGodot.Scripts.Core;
using Logger = pokemonGodot.Scripts.Core.Logger;


namespace pokemonGodot.Scripts.Gameplay.Levels
{


	public partial class Level : Node2D
	{

		[ExportCategory("Level Basics")] 
		[Export] public LevelName LevelName;

		[ExportCategory("Camera Limits")] 
		[Export] public int Top;
		
		[Export] public int Bottom;
		
		[Export] public int Left;
		
		[Export] public int Right;
		
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Logger.Info($"Loading Level {LevelName}...");	
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
}
