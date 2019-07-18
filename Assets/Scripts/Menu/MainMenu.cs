using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject Settings = null;
    [SerializeField]
    Button StartGameButton = null, ContinueButton = null, ExitButton = null, SettingsButton = null;
    [SerializeField]
    GameObject[] ObjectsToHideOnPause = null;

    BannerView Banner;

    void Start()
    {
        Prefs.UpdateCameraPrefs(Camera.main);

        StartGameButton.onClick.AddListener(() =>
        {
            if (Banner != null) Banner.Hide();

            gameObject.SetActive(false);
            Game.game.gameObject.SetActive(true);

            Game.game.StartGame();
        });

        ContinueButton.onClick.AddListener(() =>
        {
            if (Banner != null) Banner.Hide();
            Continue();

            ContinueButton.gameObject.SetActive(false);
            StartGameButton.gameObject.SetActive(true);
        });

        SettingsButton.onClick.AddListener(() =>
        {
            Settings.SetActive(true);
            gameObject.SetActive(false);
        });

        ExitButton.onClick.AddListener(Application.Quit);

        Settings.SetActive(true);
        ContinueButton.gameObject.SetActive(false);

        if (Application.platform == RuntimePlatform.WebGLPlayer) ExitButton.gameObject.SetActive(false);

#if !UNITY_EDITOR
        CreateAdBanner();
#endif
    }

    public void Pause()
    {
        ContinueButton.gameObject.SetActive(true);
        StartGameButton.gameObject.SetActive(false);
        gameObject.SetActive(true);

        if (Banner != null) Banner.Show();

        foreach (GameObject obj in ObjectsToHideOnPause) obj.SetActive(false);

        StartGameButton.GetComponent<TextMeshProUGUI>().text = "продолжить";
    }

    public void Continue()
    {
        gameObject.SetActive(false);
        Settings.gameObject.SetActive(false);

        foreach (GameObject obj in ObjectsToHideOnPause) obj.SetActive(true);

        StartGameButton.GetComponent<TextMeshProUGUI>().text = "начать";
        Game.game.ContinueGame();
    }

    void CreateAdBanner()
    {
        MobileAds.Initialize("ca-app-pub-6291991022802883~2982050741");
        Banner = new BannerView("ca-app-pub-6291991022802883/2598907369", AdSize.Banner, AdPosition.Top);
        Banner.LoadAd(new AdRequest.Builder().AddTestDevice("B6994A4369B1E37C").Build());
    }
}