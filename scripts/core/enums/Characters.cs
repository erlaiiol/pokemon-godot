using Godot;

namespace pokemonGodot.Scripts.Core.Enums
{
    public enum Characters
    {

    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionExtensions
    {
        public static Vector2 ToVector2(this Direction dir) => dir switch
        {
            Direction.Up    => Vector2.Up,
            Direction.Down  => Vector2.Down,
            Direction.Left  => Vector2.Left,
            Direction.Right => Vector2.Right,
            _               => Vector2.Zero
        };

        public static Direction Opposite(this Direction dir) => dir switch
        {
            Direction.Up    => Direction.Down,
            Direction.Down  => Direction.Up,
            Direction.Left  => Direction.Right,
            Direction.Right => Direction.Left,
            _               => Direction.None
        };
    }

    #region Characters
    public enum ECharacterAnimation
    {
        idle_down,
        
        idle_up,
        
        idle_left,
        
        idle_right,
        
        walk_down,
        
        walk_up,
        
        walk_left,
        
        walk_right,
        
        turn_down,
        
        turn_up,
        
        turn_left,
        
        turn_right
    }

    public enum ECharacterMovement
    {
        WALKING,
        JUMPING,
    }
    #endregion
}