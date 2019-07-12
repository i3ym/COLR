using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeteorEffect
{
    public static Dictionary<MeteorEffectType, MeteorEffect> Effects = new Dictionary<MeteorEffectType, MeteorEffect>();

    public Color Color { get; private set; }
    public Material Material;

    Action StartAction;
    Action EndAction;
    int EffectCount = 0;
    int WaitTime = 5000;
    bool IsPlaying = false;

    static MeteorEffect()
    {
        Effects.Add(MeteorEffectType.None, new MeteorEffect()
        {
            Color = new Color(1f, 1f, 1f, 1f),
                WaitTime = 0,
                StartAction = () => { },
                EndAction = () => { }
        });
        Effects.Add(MeteorEffectType.Speedup, new MeteorEffect()
        {
            Color = new Color(.5f, 1f, .5f, 1f),
                WaitTime = 10_000,
                StartAction = () => Game.PlayerSpeedMultiplier++,
                EndAction = () => Game.PlayerSpeedMultiplier--
        });
        Effects.Add(MeteorEffectType.Slowdown, new MeteorEffect()
        {
            Color = new Color(0f, .5f, 0f, 1f),
                WaitTime = 5_000,
                StartAction = () => Game.PlayerSpeedMultiplier--,
                EndAction = () => Game.PlayerSpeedMultiplier++
        });
        Effects.Add(MeteorEffectType.FasterShoot, new MeteorEffect()
        {
            Color = new Color(0f, 1f, 1f, 1f),
                WaitTime = 10_000,
                StartAction = () => Game.PlayerShootSpeed -= .5f,
                EndAction = () => Game.PlayerShootSpeed += .5f
        });
        Effects.Add(MeteorEffectType.SlowerShoot, new MeteorEffect()
        {
            Color = new Color(0f, 0f, 1f, 1f),
                WaitTime = 5_000,
                StartAction = () => Game.PlayerShootSpeed += .5f,
                EndAction = () => Game.PlayerShootSpeed -= .5f
        });
    }

    public async void PlayEffect()
    {
        EffectCount++;

        if (IsPlaying) return;

        IsPlaying = true;
        StartAction();

        while (EffectCount > 0)
        {
            EffectCount--;
            await Task.Delay(WaitTime);
        }

        EndAction();
        IsPlaying = false;
    }
}

public enum MeteorEffectType
{
    None,
    Speedup,
    Slowdown,
    FasterShoot,
    SlowerShoot
}