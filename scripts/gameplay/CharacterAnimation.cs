using Godot;
using System;
using System.Reflection.Metadata.Ecma335;
using pokemonGodot.Scripts.Core;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{
	public partial class CharacterAnimation : AnimatedSprite2D
	{

		[ExportCategory("Nodes")] 
		
		[Export] CharacterInput CharacterInput;
		
		[Export] CharacterMovement CharacterMovement;
		[Export] ECharacterAnimation ECharacterAnimation = ECharacterAnimation.idle_down;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			CharacterMovement.Animation += PlayAnimation;
			Logger.Info("Loading player animation component...");
		}


		public void PlayAnimation(string animationType)
		{
			ECharacterAnimation previousAnimation = ECharacterAnimation;
			
			if (CharacterMovement.IsMoving()) return;

			switch (animationType)
			{
				case "walk": 
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.walk_up;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.walk_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.walk_right;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.walk_down;
					}
					break;
				case "turn":
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.turn_up;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.turn_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.turn_right;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.turn_down;
					}

					break;
				case "idle":
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.idle_up;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.idle_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.idle_right;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.idle_down;
					}

					break;
			}

			if (previousAnimation == ECharacterAnimation) return;
			
				Logger.Info($"Animation {previousAnimation} was changed to {ECharacterAnimation}");
				Play(ECharacterAnimation.ToString());


		}
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}
	}
}
