using UnityEngine;

public abstract class Movable : MonoBehaviour
{
    public abstract float SizeX { get; }
    public abstract float SizeY { get; }

    public new RectTransform transform { get; protected set; }

    public bool IsAlive = true;
    public Vector2 Direction;

    void Awake()
    {
        transform = gameObject.transform as RectTransform;
        transform.sizeDelta = new Vector2(SizeX, SizeY);
    }

    public virtual void Death()
    {
        IsAlive = false;
        Game.game.Movables.Remove(this);
    }
}