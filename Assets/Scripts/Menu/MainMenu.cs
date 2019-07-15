using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject Settings = null;
    [SerializeField]
    Button StartGameButton = null, ExitButton = null, SettingsButton = null;

    public BannerView Banner;

    void Start()
    {
        Prefs.UpdateCameraPrefs(Camera.main);

        StartGameButton.onClick.AddListener(() =>
        {
            if (Banner != null) Banner.Hide();

            gameObject.SetActive(false);
            Game.game.gameObject.SetActive(true);

            if (Game.IsPaused) Game.game.ContinueGame();
            else Game.game.StartGame();
        });

        ExitButton.onClick.AddListener(Application.Quit);

        SettingsButton.onClick.AddListener(() =>
        {
            Settings.SetActive(true);
            gameObject.SetActive(false);
        });

        Settings.SetActive(true);

#if !UNITY_EDITOR
        CreateAdBanner();
#endif
    }

    void CreateAdBanner()
    {
        MobileAds.Initialize("ca-app-pub-6291991022802883~2982050741");
        Banner = new BannerView("ca-app-pub-6291991022802883/2598907369", AdSize.Banner, AdPosition.Top);
        Banner.LoadAd(new AdRequest.Builder().AddTestDevice("B6994A4369B1E37C").Build());
    }
}