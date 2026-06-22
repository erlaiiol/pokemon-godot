using Godot;
using Godot.Collections;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.UI;
using System;
using Logger = pokemonGodot.Scripts.Core.Logger;


namespace pokemonGodot.Scripts.Gameplay.Levels
{

	[Tool]
	public partial class Sign : StaticBody2D
	{

		[Export]
		public Array<string> Messages = [];

		private SignType _signStyle = SignType.METAL;

		[Export] public SignType SignStyle
		{
			get => _signStyle;
			set
			{
				if (_signStyle != value)
				{
					_signStyle = value;
					UpdateSprite();
				}
			}
		}

		private Sprite2D _sprite2D;

		private readonly Dictionary<SignType, AtlasTexture> _textures = new()
		{
			{SignType.METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_metal.tres")},
			{SignType.WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_wood.tres")},
		};

		public override void _Ready()
		{
			_sprite2D ??= GetNode<Sprite2D>("Sprite2D");
			UpdateSprite();	
		}

		private void UpdateSprite()
		{
			if ( _sprite2D == null)
			{
				_sprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");

				if (_sprite2D == null)
				{
					Logger.Error("Sprite2D node not found in Sign node.");
					return;
				}
			}
			if (_textures.TryGetValue(SignStyle, out var texture))
			{
				_sprite2D.Texture = texture;
			}
			else
			{
				Logger.Error($"Texture for SignType {SignStyle} not found.");
				_sprite2D.Texture = null;
			}

		}

		public void PlayMessage()
		{
			MessageManager.PlayText([..Messages]);
		}
	}
}
