using Godot;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Utilities
{
    public abstract partial class State : Node
    {
        [Export] public Node StateOwner;

        public virtual void EnterState()
        {
            Logger.Info($"Entering {GetType().Name} state ...");
        }

        public virtual void ExitState()
        {
            Logger.Info($"Exiting {GetType().Name} state ...");
        }
    }
}