using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    PostProcessVolume PPP = null;
    [SerializeField]
    GameObject SettingsObj = null, MainMenu = null, Graphics = null;
    [SerializeField]
    Button GraphicsButton = null, BackButton = null;
    [SerializeField]
    Button GraphicsBloomButton = null, GraphicsGrainButton = null, GraphicsChromaButton = null, GraphicsLensButton = null, GraphicsBackButton = null;

    void Start()
    {
        MainMenu.SetActive(true);
        Graphics.SetActive(false);
        SettingsObj.SetActive(false);

        GraphicsButton.onClick.AddListener(() =>
        {
            Graphics.SetActive(true);
            SettingsObj.SetActive(false);
        });
        BackButton.onClick.AddListener(() =>
        {
            MainMenu.SetActive(true);
            SettingsObj.SetActive(false);
        });
        GraphicsBackButton.onClick.AddListener(() =>
        {
            SettingsObj.SetActive(true);
            Graphics.SetActive(false);
        });

        var bloom = PPP.profile.GetSetting<Bloom>();
        GraphicsBloomButton.onClick.AddListener(() => bloom.active = !bloom.active);

        var grain = PPP.profile.GetSetting<Grain>();
        GraphicsGrainButton.onClick.AddListener(() => grain.active = !grain.active);

        var chroma = PPP.profile.GetSetting<ChromaticAberration>();
        GraphicsChromaButton.onClick.AddListener(() => chroma.active = !chroma.active);

        var lens = PPP.profile.GetSetting<LensDistortion>();
        GraphicsLensButton.onClick.AddListener(() => lens.active = !lens.active);
    }
}