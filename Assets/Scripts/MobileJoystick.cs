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

        var pos = Game.Camera.ScreenToViewportPoint(Input.mousePosition);

        pos.x *= Screen.width * 2f;
        pos.x -= Screen.width * 2f;
        pos.y *= Screen.height * 2f;

        var diff = (Vector2) pos - ((Vector2) transform.anchoredPosition + new Vector2(-transform.rect.width, transform.rect.height) / 2f);

        if (diff.x > transform.rect.width / 2f) diff.x = transform.rect.width / 2f;
        else if (diff.x < -transform.rect.width / 2f) diff.x = -transform.rect.width / 2f;

        if (diff.y > transform.rect.height / 2f) diff.y = transform.rect.height / 2f;
        else if (diff.y < -transform.rect.height / 2f) diff.y = -transform.rect.height / 2f;

        Knob.anchoredPosition = new Vector2(diff.x, diff.y);

        Vertical = Knob.anchoredPosition.y / transform.rect.height * 2f / 4f;
        Horizontal = Knob.anchoredPosition.x / transform.rect.width * 2f;
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