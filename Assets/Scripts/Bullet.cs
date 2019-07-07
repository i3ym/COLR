using UnityEngine;

public class Bullet : Movable
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf || !collision.gameObject.activeSelf) return;

        collision.gameObject.GetComponent<Meteor>().Death();
        Death();
    }

    public override void Death()
    {
        base.Death();
        Game.game.BulletPool.Enqueue(rigidbody);
    }
}