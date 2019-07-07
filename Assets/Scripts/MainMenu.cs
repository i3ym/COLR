using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject Field;
    [SerializeField]
    Button StartGameButton = null, ExitButton = null, OffEffectsButton = null;

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

        OffEffectsButton.onClick.AddListener(() =>
        {
            Destroy(Game.Camera.gameObject.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>());
        });
    }
}