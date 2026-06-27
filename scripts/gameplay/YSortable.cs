using Godot;
using pokemonGodot.Scripts.Core;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Gameplay
{
	public partial class YSortable : Node
	{
		// Pour les nœuds individuels (NPC, arbres-scènes) : laisser false.
		// Le seuil est automatiquement la position Y du parent dans le monde.
		//
		// Pour les TileMapLayer (position toujours 0,0) : mettre true et configurer
		// ThresholdY manuellement à la valeur Y en pixels du point de bascule voulu.
		[Export] public bool UseFixedThreshold = false;
		[Export] public float ThresholdY = 0f;

		private Node2D _parent;
		private float _effectiveThreshold;

		public override void _Ready()
		{
			_parent = GetParent<Node2D>();
			_effectiveThreshold = UseFixedThreshold ? ThresholdY : _parent.Position.Y;

			Logger.Info($"YSortable sur '{_parent.Name}' — seuil Y : {_effectiveThreshold} (mode: {(UseFixedThreshold ? "fixe" : "position parent")})");
		}

		public override void _Process(double delta)
		{
			var player = GameManager.GetPlayer();
			if (player == null) return;

			_parent.ZIndex = player.Position.Y <= _effectiveThreshold
				? ZLayer.InFront
				: ZLayer.Behind;
		}
	}
}
