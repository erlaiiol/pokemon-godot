using Godot;
using pokemonGodot.Scripts.UI;
using pokemonGodot.Scripts.Utilities;

namespace pokemonGodot.Scripts.Gameplay.States
{

	public partial class PlayerMessageState : State
	{
		
		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (!MessageManager.Scrolling() && Input.IsActionJustReleased("use"))
			{
				MessageManager.ScrollText();

				if (MessageManager.GetMessages().Count == 0)
				{
					StateMachine.ChangeState("Roam");
					
				}
			}
		}
	}

	
}
