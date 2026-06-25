using Godot;
using System;
using pokemonGodot.Scripts.Core.Enums;


namespace pokemonGodot.Scripts.Gameplay.Levels
{
	public partial class SpawnPoint : Node2D
	{
		public override void _EnterTree()
		{
			AddToGroup(LevelGroups.SPAWNPOINTS.ToString());
		}

		public override void _ExitTree()
		{
			RemoveFromGroup(LevelGroups.SPAWNPOINTS.ToString());
		}
	}
	
}
