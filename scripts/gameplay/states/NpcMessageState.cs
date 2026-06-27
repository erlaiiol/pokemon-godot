using Godot;
using pokemonGodot.Scripts.Core;
using pokemonGodot.Scripts.Utilities;


namespace pokemonGodot.Scripts.Gameplay.States
{
	public partial class NpcMessageState : State
	{
		public override void _Ready()
		{
			Signals.Instance.MessageBoxOpen += (value) =>
			{
				if (!value)
				{
					StateMachine.ChangeState("Roam");
				}
			};
		}
	}
}
