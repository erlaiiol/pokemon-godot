using System;
using Godot;
using pokemonGodot.Scripts.Core.Enums;

namespace pokemonGodot.Scripts.Gameplay {

    public abstract partial class CharacterInput : Node
    {
        [Signal]
        public delegate void WalkEventHandler();

        [Signal]
        public delegate void TurnEventHandler();

        [ExportCategory("Common Input")]

        [Export] public Direction Direction = Direction.None;
    }
}
