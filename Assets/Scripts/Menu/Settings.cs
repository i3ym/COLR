using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    GameObject SettingsObj = null, MainMenu = null, Graphics = null, Audio = null;
    [SerializeField]
    Button GraphicsButton = null, LanguageButton = null, AudioButton = null, BackButton = null;
    [SerializeField]
    Button GraphicsBloomButton = null, GraphicsGrainButton = null, GraphicsChromaButton = null, GraphicsLensButton = null, GraphicsBackButton = null;
    [SerializeField]
    Slider AudioMusicSlider = null, AudioSoundsSlider = null;
    [SerializeField]
    Button AudioBackButton = null;

    void Start()
    {
        MainMenu.SetActive(true);
        Graphics.SetActive(false);
        SettingsObj.SetActive(false);
        Audio.SetActive(false);

        GraphicsButton.onClick.AddListener(() =>
        {
            Graphics.SetActive(true);
            SettingsObj.SetActive(false);
        });
        LanguageButton.onClick.AddListener(() => Game.SetLanguage((Language) ((((int) Prefs.Lang) + 1) % Enum.GetValues(typeof(Language)).Length)));
        AudioButton.onClick.AddListener(() =>
        {
            AudioMusicSlider.value = Game.game.Music.volume;
            AudioSoundsSlider.value = Game.game.Player.ShootSound.volume;

            Audio.SetActive(true);
            SettingsObj.SetActive(false);
        });

        BackButton.onClick.AddListener(() =>
        {
            MainMenu.SetActive(true);
            SettingsObj.SetActive(false);

            Game.game.SaveSettings();
        });

        ///

        GraphicsBloomButton.onClick.AddListener(() => TurnOption(ref Prefs.Bloom));
        GraphicsGrainButton.onClick.AddListener(() => TurnOption(ref Prefs.Grain));
        GraphicsChromaButton.onClick.AddListener(() => TurnOption(ref Prefs.Chroma));
        GraphicsLensButton.onClick.AddListener(() => TurnOption(ref Prefs.Lens));

        ///

        GraphicsBackButton.onClick.AddListener(() =>
        {
            SettingsObj.SetActive(true);
            Graphics.SetActive(false);
        });

        AudioMusicSlider.onValueChanged.AddListener((value) => Game.game.Music.volume = value);
        AudioSoundsSlider.onValueChanged.AddListener((value) => Game.game.Player.ShootSound.volume = value);

        AudioBackButton.onClick.AddListener(() =>
        {
            SettingsObj.SetActive(true);
            Audio.SetActive(false);
        });
    }

    static void TurnOption(ref bool opt)
    {
        opt = !opt;
        Prefs.UpdateCameraPrefs(Game.Camera);

        Game.game.SaveSettings();
    }
}