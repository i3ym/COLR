using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject Settings = null;
    [SerializeField]
    Button StartGameButton = null, ExitButton = null, SettingsButton = null;

    BannerView Banner;

    void Start()
    {
        StartGameButton.onClick.AddListener(() =>
        {
            Game.isPlaying = true;
            Banner.Hide();

            SceneManager.LoadScene(1);
        });

        ExitButton.onClick.AddListener(Application.Quit);

        SettingsButton.onClick.AddListener(() =>
        {
            Settings.SetActive(true);
            gameObject.SetActive(false);
        });

        Settings.SetActive(true);

        CreateAdBanner();
    }

    void CreateAdBanner()
    {
        MobileAds.Initialize("ca-app-pub-6291991022802883~2982050741");
        Banner = new BannerView("ca-app-pub-6291991022802883/2598907369", AdSize.Banner, AdPosition.Top);
        Banner.LoadAd(new AdRequest.Builder().AddTestDevice("B6994A4369B1E37C").Build());
    }
}