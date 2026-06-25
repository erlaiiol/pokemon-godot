using Godot;
using Logger = pokemonGodot.Scripts.Core.Logger;

namespace pokemonGodot.Scripts.Utilities
{
    public abstract partial class State : Node
    {
        [Export] public Node StateOwner;
        [Export] public StateMachine StateMachine;

        public virtual void EnterState()
        {
            Logger.Info($" {StateOwner.Name} Entering {GetType().Name} state ...");
        }

        public virtual void ExitState()
        {
            Logger.Info($" {StateOwner.Name} Exiting {GetType().Name} state ...");
        }
    }
}