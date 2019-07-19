using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
public class Prefs
{
    public bool Bloom, Grain, Chroma, Lens, Particles;
    public Language Lang;
    public float MusicVolume, SoundsVolume;

    static Prefs Preferences = new Prefs();

    public Prefs()
    {
        Bloom = Grain = Chroma = Lens = Particles = true;
        MusicVolume = SoundsVolume = 1f;

        if (!Enum.TryParse<Language>(Application.systemLanguage.ToString(), out Lang))
            Lang = Language.English;
    }

    public void Update()
    {
        var profile = Game.Camera.GetComponent<PostProcessVolume>().profile;

        profile.GetSetting<Bloom>().active = Bloom;
        profile.GetSetting<Grain>().active = Grain;
        profile.GetSetting<ChromaticAberration>().active = Chroma;
        profile.GetSetting<LensDistortion>().active = Lens;

        Game.game.Music.volume = MusicVolume;
        Game.game.Player.ShootSound.volume = SoundsVolume;
    }
}