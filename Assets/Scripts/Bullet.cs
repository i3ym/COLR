using UnityEngine;

public class Bullet : Movable
{
    public override void Death()
    {
        base.Death();
        Game.game.BulletPool.Enqueue(this);
    }
}