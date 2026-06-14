using Godot;
using System;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{


	public partial class CharacterCollisionRayCast : RayCast2D
	{

		[Signal]
		public delegate void CollisionEventHandler(bool collided);

		[ExportCategory("Collision Vars")] 
		
		[Export] public CharacterInput CharacterInput;
		
		[Export] public GodotObject Collider;
		
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Logger.Info("Loading CharacterCollisionRayCast...");
		}

		
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			Collider = IsColliding() ? GetCollider() : null;
		}

		public bool CheckCollisionInDirection(Vector2 direction)
		{
			TargetPosition = direction;
			ForceRaycastUpdate();
			return IsColliding();
		}
	}
	
}
