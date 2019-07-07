using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour
{
    [SerializeField]
    RectTransform Knob = null;
    [SerializeField]
    EventTrigger ShootTrigger = null;

    [HideInInspector]
    public float Horizontal, Vertical;
    EventTrigger Trigger;
    new RectTransform transform;
    bool Dragging = false;

    void Start()
    {
        transform = gameObject.transform as RectTransform;

        Trigger = GetComponent<EventTrigger>();
        if (Trigger == null) Trigger = gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => Dragging = true);
        Trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) =>
        {
            Dragging = false;
            Knob.anchoredPosition = Vector2.zero;
            Vertical = Horizontal = 0f;
        });
        Trigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) =>
        {
            Game.game.RestartGameIfNeeded();
            StartCoroutine(ShootCoroutine());
        });
        ShootTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => StopAllCoroutines());
        ShootTrigger.triggers.Add(entry);
    }

    void Update()
    {
        if (!Dragging) return;

        Vector2 pos = (Vector2) Game.Camera.ScreenToViewportPoint(Input.mousePosition) * Game.game.gamePlaceholder.rect.size;

        pos -= transform.anchoredPosition;
        pos.x -= Game.game.gamePlaceholder.rect.width - transform.rect.width;
        pos -= transform.rect.size / 2f;

        if (pos.x > transform.rect.width / 2f) pos.x = transform.rect.width / 2f;
        else if (pos.x < -transform.rect.width / 2f) pos.x = -transform.rect.width / 2f;

        if (pos.y > transform.rect.height / 2f) pos.y = transform.rect.height / 2f;
        else if (pos.y < -transform.rect.height / 2f) pos.y = -transform.rect.height / 2f;

        Knob.anchoredPosition = new Vector2(pos.x, pos.y);

        Vertical = pos.y / transform.rect.height * 2f / 4f;
        Horizontal = pos.x / transform.rect.width * 2f;
    }

    IEnumerator ShootCoroutine()
    {
        while (true)
        {
            Game.Player.Shoot();
            yield return null;
        }
    }
}