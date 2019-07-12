using System.Collections;
using UnityEngine;

public class Meteor : Movable
{
    public MeteorEffect Effect { get => MeteorEffect.Effects[EffectType]; }
    public MeteorEffectType EffectType = MeteorEffectType.None;

    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf || !collision.gameObject.activeSelf) return;

        if (EffectType != MeteorEffectType.None && !collision.CompareTag("Meteor"))
        {
            Effect.PlayEffect();

            Death();
            return;
        }

        if (collision.CompareTag("Player")) Game.game.GameOver();
        else if (collision.CompareTag("Meteor"))
        {
            collision.gameObject.GetComponent<Meteor>().Death();
            Death();
            Game.game.SpawnMeteorNextFrame();
        }
    }

    public override void Death()
    {
        base.Death();
        Game.game.MeteorPool.Enqueue(rigidbody);

        Game.game.PlayDeathParticle(transform.position, Effect.Color);
    }
}