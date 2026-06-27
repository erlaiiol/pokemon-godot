using System;
using Godot;
using Godot.Collections;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Core.Enums;
using pokemonGodot.Scripts.UI;
using pokemonGodot.Scripts.Utilities;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{
	[Tool]
	public partial class Npc : CharacterBody2D
	{
		private NpcAppearance npcAppearance = NpcAppearance.Worker;

		[ExportCategory("Traits")]
		[Export] NpcAppearance NpcAppearance
		{
			get => npcAppearance;
			set
			{
				if (npcAppearance != value)
				{
					npcAppearance = value;
					UpdateAppearance();
				}
			}
		}

		private AnimatedSprite2D animatedSprite2D;
		private NpcInput npcInput;
		private StateMachine stateMachine;

		private CharacterMovement characterMovement;

		private readonly Dictionary<NpcAppearance, SpriteFrames> appearanceFrames = new()
		{
			{ NpcAppearance.Worker, GD.Load<SpriteFrames>("res://resources/spriteframes/worker.tres") },
			{ NpcAppearance.Gardener, GD.Load<SpriteFrames>("res://resources/spriteframes/gardener.tres") },
			{ NpcAppearance.BugCatcher, GD.Load<SpriteFrames>("res://resources/spriteframes/bug_catcher.tres") },
		};

		[Export]
		public NpcInputConfig NpcInputConfig;

		public override void _Ready()
		{
			if (Engine.IsEditorHint())
			{
				UpdateAppearance();
				return;
			}

			npcInput ??= GetNode<NpcInput>("Input");
			npcInput.Config = NpcInputConfig;

			stateMachine ??= GetNode<StateMachine>("StateMachine");
			stateMachine.ChangeState("Roam");

			animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			characterMovement ??= GetNode<CharacterMovement>("Movement");

			GlobalPosition = Globals.SnapToGrid(GlobalPosition);
		}

		public override void _Process(double delta)
		{
			if (Engine.IsEditorHint()) return;
		}
		private void UpdateAppearance()
		{
			Logger.Info("Changing appearance in the editor");
			if (animatedSprite2D == null)
			{
				animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
				if (animatedSprite2D == null)
				{
					return;
				}
			}

			if (appearanceFrames.TryGetValue(npcAppearance, out var spriteFrames))
			{
				animatedSprite2D.SpriteFrames = spriteFrames;
			}
			else
			{
				animatedSprite2D.SpriteFrames = null;
			}
		}

		public void PlayMessage(Direction playerDirection)
		{
			if (Engine.IsEditorHint()) return;
			if (characterMovement.IsMoving()) return;

			Direction faceTowardPlayer = playerDirection.Opposite();
			if (npcInput.Direction != faceTowardPlayer)
			{
				npcInput.Direction = faceTowardPlayer;
				npcInput.EmitSignal(CharacterInput.SignalName.Turn);
			}

			stateMachine.ChangeState("Message");
			MessageManager.PlayText([.. NpcInputConfig.Messages]);
		}
	}
	
}
