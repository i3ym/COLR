using TMPro;
using UnityEngine;

public class OffScreen : MonoBehaviour
{
    [SerializeField]
    MonoBehaviour ControlScript;
    RectTransform shadowGO;
    new RectTransform transform;
    TextMeshProUGUI scoreText;

    void Start()
    {
        transform = GetComponent<RectTransform>();

        shadowGO = Instantiate(gameObject, transform.parent).GetComponent<RectTransform>();
        scoreText = shadowGO.GetChild(1).GetComponent<TextMeshProUGUI>();
        Destroy(shadowGO.GetComponent<OffScreen>());
        if (ControlScript != null) Destroy(shadowGO.GetComponent(ControlScript.GetType()));
    }
    void Update() => scoreText.text = Game.Score.ToString();
    void FixedUpdate()
    {
        if (Mathf.Abs(transform.anchoredPosition.y) > Mathf.Abs(transform.anchoredPosition.x))
        {
            if (transform.anchoredPosition.y > 0) shadowGO.anchorMax = shadowGO.anchorMin = new Vector2(.5f, -.5f);
            else shadowGO.anchorMax = shadowGO.anchorMin = new Vector2(.5f, 1.5f);
        }
        else
        {
            if (transform.anchoredPosition.x > 0) shadowGO.anchorMax = shadowGO.anchorMin = new Vector2(-.5f, .5f);
            else shadowGO.anchorMax = shadowGO.anchorMin = new Vector2(1.5f, .5f);
        }

        shadowGO.anchoredPosition = transform.anchoredPosition;
        shadowGO.rotation = transform.rotation;

        //TODO optimize (lol)
        if (transform.anchoredPosition.x > Game.canvasSize.x / 2) transform.anchoredPosition -= new Vector2(Game.canvasSize.x, 0f);
        else if (transform.anchoredPosition.x < -Game.canvasSize.x / 2) transform.anchoredPosition += new Vector2(Game.canvasSize.x, 0f);

        if (transform.anchoredPosition.y > Game.canvasSize.y / 2) transform.anchoredPosition -= new Vector2(0f, Game.canvasSize.y);
        else if (transform.anchoredPosition.y < -Game.canvasSize.y / 2) transform.anchoredPosition += new Vector2(0f, Game.canvasSize.y);
    }

    void OnDestroy()
    {
        if (shadowGO) Destroy(shadowGO.gameObject);
    }
}