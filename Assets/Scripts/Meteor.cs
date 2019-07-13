using System.Collections;
using UnityEngine;

public class Meteor : Movable
{
    [HideInInspector]
    public MeteorEffect Effect { get => MeteorEffect.Effects[EffectType]; }
    public MeteorEffectType EffectType = MeteorEffectType.None;

    IEnumerator SpawnMeteorNextFixedFrameCoroutine()
    {
        yield return new WaitForFixedUpdate();
        Game.game.SpawnMeteor();
    }

    public override void Death()
    {
        base.Death();

        Game.game.MeteorPool.Enqueue(this);
        Game.game.PlayDeathParticle(transform.position, Effect.Color);

        if (Game.IsPlaying) Game.game.SpawnMeteorNextFrame();
    }
}