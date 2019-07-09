using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class Prefs
{
    public static bool Bloom, Grain, Chroma, Lens;

    static Prefs()
    {
        Bloom = Grain = Chroma = Lens = true;
    }

    public static void UpdateCameraPrefs(Camera camera)
    {
        var profile = camera.GetComponent<PostProcessVolume>().profile;

        profile.GetSetting<Bloom>().active = Bloom;
        profile.GetSetting<Grain>().active = Grain;
        profile.GetSetting<ChromaticAberration>().active = Chroma;
        profile.GetSetting<LensDistortion>().active = Lens;
    }
}