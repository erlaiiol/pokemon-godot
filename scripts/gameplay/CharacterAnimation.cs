using Godot;
using pokemonGodot.Scripts.Core.Enums;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{
	public partial class CharacterAnimation : AnimatedSprite2D
	{
		[ExportCategory("Nodes")]

		[Export] CharacterInput CharacterInput;

		[Export] CharacterMovement CharacterMovement;
		[Export] ECharacterAnimation ECharacterAnimation = ECharacterAnimation.idle_down;

		// Permet de reprendre l'animation walk là où elle s'est arrêtée (case précédente),
		// au lieu de repartir de frame 0 à chaque nouvelle case. Sans ça, l'animation
		// ne progresse jamais au-delà des premières frames quand PatrolMoveInterval = 0.
		private int _savedWalkFrame = 0;
		private ECharacterAnimation _savedWalkAnimation = ECharacterAnimation.idle_down;

		public override void _Ready()
		{
			CharacterMovement.Animation += PlayAnimation;
			// AnimationFinished = animation non-loopée terminée (jamais déclenché pour loop=true)
			// AnimationLooped   = animation loopée qui termine un cycle → c'est celui-ci qu'on veut
			AnimationLooped += OnTurnCycleCompleted;
			Logger.Info("Loading player animation component...");
		}

		private void OnTurnCycleCompleted()
		{
			if (ECharacterAnimation is ECharacterAnimation.turn_up
				or ECharacterAnimation.turn_down
				or ECharacterAnimation.turn_left
				or ECharacterAnimation.turn_right)
			{
				PlayAnimation("idle");
			}
		}

		public void PlayAnimation(string animationType)
		{
			ECharacterAnimation previousAnimation = ECharacterAnimation;

			if (CharacterMovement.IsMoving()) return;

			ECharacterAnimation = (animationType, CharacterInput.Direction) switch
			{
				("walk",  Direction.Up)    => ECharacterAnimation.walk_up,
				("walk",  Direction.Down)  => ECharacterAnimation.walk_down,
				("walk",  Direction.Left)  => ECharacterAnimation.walk_left,
				("walk",  Direction.Right) => ECharacterAnimation.walk_right,
				("turn",  Direction.Up)    => ECharacterAnimation.turn_up,
				("turn",  Direction.Down)  => ECharacterAnimation.turn_down,
				("turn",  Direction.Left)  => ECharacterAnimation.turn_left,
				("turn",  Direction.Right) => ECharacterAnimation.turn_right,
				("idle",  Direction.Up)    => ECharacterAnimation.idle_up,
				("idle",  Direction.Down)  => ECharacterAnimation.idle_down,
				("idle",  Direction.Left)  => ECharacterAnimation.idle_left,
				("idle",  Direction.Right) => ECharacterAnimation.idle_right,
				_ => ECharacterAnimation
			};

			if (previousAnimation == ECharacterAnimation) return;

			// Passage walk → idle : on sauvegarde la frame pour la reprendre plus tard.
			if (animationType == "idle" && previousAnimation.ToString().StartsWith("walk"))
			{
				_savedWalkFrame = Frame;
				_savedWalkAnimation = previousAnimation;
			}

			Logger.Info($"Animation {previousAnimation} was changed to {ECharacterAnimation}");
			Play(ECharacterAnimation.ToString());

			// Reprise du même walk (même direction) après un idle inter-cases :
			// on restaure la frame sauvegardée pour que le cycle stride soit continu.
			if (animationType == "walk" && ECharacterAnimation == _savedWalkAnimation)
			{
				int frameCount = SpriteFrames.GetFrameCount(ECharacterAnimation.ToString());
				if (frameCount > 0)
					Frame = _savedWalkFrame % frameCount;
			}
		}

		public override void _Process(double delta) { }
	}
}
