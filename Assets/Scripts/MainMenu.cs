using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject Field;
    [SerializeField]
    Button StartGameButton = null, ExitButton = null;

    void Start()
    {
        StartGameButton.onClick.AddListener(() =>
        {
            Field.SetActive(true);
            gameObject.SetActive(false);
            Game.isPlaying = true;

            Game.Banner.Hide();
        });
        ExitButton.onClick.AddListener(Application.Quit);
    }
}