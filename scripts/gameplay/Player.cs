using Godot;
using System;
using pokemonGodot.Scripts.Utilities;

public partial class Player : CharacterBody2D
{
	[Export] public StateMachine StateMachine;

	public override void _Ready()
	{
		StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
	}
}
