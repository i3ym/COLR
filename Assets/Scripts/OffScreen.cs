using TMPro;
using UnityEngine;

public class OffScreen : MonoBehaviour
{
    [HideInInspector]
    public RectTransform shadowGO;
    new RectTransform transform;
    TextMeshProUGUI scoreText;

    Vector2 _pos = new Vector2();

    void Start()
    {
        transform = gameObject.transform as RectTransform;

        shadowGO = Instantiate(gameObject, transform.parent).GetComponent<RectTransform>();
        scoreText = shadowGO.GetChild(0).GetComponent<TextMeshProUGUI>();

        Destroy(shadowGO.GetComponent<OffScreen>());
        Destroy(shadowGO.GetComponent<Player>());
        Destroy(shadowGO.GetComponent<AudioSource>());
    }

    void Update() => scoreText.text = Game.game.Score.ToString();

    void FixedUpdate()
    {
        shadowGO.rotation = transform.rotation;

        _pos = transform.anchoredPosition;

        if (Mathf.Abs(transform.anchoredPosition.y) > Mathf.Abs(transform.anchoredPosition.x))
        {
            if (transform.anchoredPosition.y > 0) _pos.y -= Game.game.GamePlaceholder.rect.height;
            else _pos.y += Game.game.GamePlaceholder.rect.height;
        }
        else
        {
            if (transform.anchoredPosition.x > 0) _pos.x -= Game.game.GamePlaceholder.rect.width;
            else _pos.x += Game.game.GamePlaceholder.rect.width;
        }

        shadowGO.anchoredPosition = _pos;

        if (transform.anchoredPosition.x > Game.game.GamePlaceholder.rect.size.x / 2) transform.anchoredPosition -= new Vector2(Game.game.GamePlaceholder.rect.size.x, 0f);
        else if (transform.anchoredPosition.x < -Game.game.GamePlaceholder.rect.size.x / 2) transform.anchoredPosition += new Vector2(Game.game.GamePlaceholder.rect.size.x, 0f);

        if (transform.anchoredPosition.y > Game.game.GamePlaceholder.rect.size.y / 2) transform.anchoredPosition -= new Vector2(0f, Game.game.GamePlaceholder.rect.size.y);
        else if (transform.anchoredPosition.y < -Game.game.GamePlaceholder.rect.size.y / 2) transform.anchoredPosition += new Vector2(0f, Game.game.GamePlaceholder.rect.size.y);
    }

    void OnDestroy()
    {
        if (shadowGO != null && shadowGO.gameObject) Destroy(shadowGO.gameObject);
    }
}