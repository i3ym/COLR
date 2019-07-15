using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    GameObject SettingsObj = null, MainMenu = null, Graphics = null;
    [SerializeField]
    Button GraphicsButton = null, BackButton = null;
    [SerializeField]
    Button GraphicsBloomButton = null, GraphicsGrainButton = null, GraphicsChromaButton = null, GraphicsLensButton = null, GraphicsBackButton = null;

    static Camera Camera;

    void Start()
    {
        Camera = Camera.main;

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

        GraphicsBloomButton.onClick.AddListener(() => TurnOption(ref Prefs.Bloom));
        GraphicsGrainButton.onClick.AddListener(() => TurnOption(ref Prefs.Grain));
        GraphicsChromaButton.onClick.AddListener(() => TurnOption(ref Prefs.Chroma));
        GraphicsLensButton.onClick.AddListener(() => TurnOption(ref Prefs.Lens));
    }

    static void TurnOption(ref bool opt)
    {
        opt = !opt;
        Prefs.UpdateCameraPrefs(Camera);

        Game.game.SaveSettings();
    }
}