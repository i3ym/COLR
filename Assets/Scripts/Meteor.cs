using System.Collections;
using UnityEngine;

public class Meteor : Movable
{
    public override float SizeX => 30f;
    public override float SizeY => SizeX;

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

        Effect.IsPlaying = false;
        Game.game.PlayDeathParticle(Position, Effect.Color);

        if (Game.IsPlaying) Game.game.SpawnMeteorNextFrame();
    }
}