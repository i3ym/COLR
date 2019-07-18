using UnityEngine;

public abstract class Movable
{
    public abstract float SizeX { get; }
    public abstract float SizeY { get; }

    public bool IsAlive = true;
    public Vector2 Position, Direction;

    public virtual void Death()
    {
        IsAlive = false;
        Game.game.Movables.Remove(this);
    }
}