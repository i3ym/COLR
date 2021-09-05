using System.Collections;
using UnityEngine;

public class Meteor : Movable
{
    public override float SizeX => 30f;
    public override float SizeY => SizeX;

    public MeteorEffect Effect => MeteorEffect.Effects[EffectType];
    public MeteorEffectType EffectType = MeteorEffectType.None;

    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    public override void Death()
    {
        base.Death();

        Effect.IsPlaying = false;
        Game.game.PlayDeathParticle(transform.anchoredPosition, Effect.Color);

        if (Game.IsPlaying) Game.game.SpawnMeteorNextFrame();
    }

    void OnTriggerEnter(Collider other)
    {
        var movable = other.GetComponent<Movable>();

        if (movable)
        {
            Death();
            movable.Death();
        }
        else
        {
            var player = other.GetComponent<Player>();

            if (player)
            {
                Effect.PlayEffect();
                Death();
            }
        }
    }
}