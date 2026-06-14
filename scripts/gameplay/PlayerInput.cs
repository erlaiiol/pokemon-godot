using Godot;
using System;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{

	public partial class PlayerInput : CharacterInput
	{
		[ExportCategory("Player Input")] [Export]
		public double HoldThreshold = 0.1f;

		[Export] public double HoldTime = 0.1f;

		// Called when the node enters the scene tree for the first time.
		public void _Ready()
		{
			Logger.Info("Loading player input component...");
		}
	}
}
