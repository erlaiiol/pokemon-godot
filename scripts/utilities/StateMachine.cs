using Godot;
using System;

namespace pokemonGodot.Scripts.Utilities
{
	public partial class StateMachine : Node
	{
		[ExportCategory("State Machine Vars")]
		
		[Export] public Node Customer;

		[Export] public State CurrentState;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			foreach (Node child in GetChildren())
			{
				if (child is State state)
				{
					state.StateOwner = Customer;
					state.StateMachine = this;
					state.SetProcess(false);
				}
			}
		}

		public string GetCurrentState()
		{
			return CurrentState.Name.ToString();
		}
		
		public void ChangeState(State newState)
		{
			CurrentState?.ExitState();
			CurrentState = newState;
			CurrentState?.EnterState();

			foreach (Node child in GetChildren())
			{
				if (child is State state)
				{
					state.SetProcess(child == CurrentState);
				}
			}
		}

		public void ChangeState(string newState)
		{
			var _state = GetNode<State>(newState);

			CurrentState?.ExitState();
			CurrentState = _state;
			CurrentState?.EnterState();

			foreach (Node child in GetChildren())
			{
				if (child is State state)
				{
					state.SetProcess(child == CurrentState);
				}
			}
		}
	}
	
}
