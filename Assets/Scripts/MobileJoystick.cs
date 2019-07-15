using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour
{
    public static float Horizontal, Vertical;

    [SerializeField]
    RectTransform Knob = null;
    [SerializeField]
    EventTrigger ShootTrigger = null;

    EventTrigger Trigger;
    new RectTransform transform;
    bool Dragging = false;
    Vector2 TouchPos;

    void Start()
    {
        Horizontal = Vertical = 0f;

        transform = gameObject.transform as RectTransform;
        TouchPos = Game.Camera.WorldToScreenPoint(Knob.position);

        Trigger = GetComponent<EventTrigger>();
        if (Trigger == null) Trigger = gameObject.AddComponent<EventTrigger>();

        void AddCallback(EventTriggerType type, UnityAction<BaseEventData> action, EventTrigger trigger = null)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(action);

            if (trigger == null) Trigger.triggers.Add(entry);
            else trigger.triggers.Add(entry);
        }

        AddCallback(EventTriggerType.PointerDown, (data) =>
        {
            Dragging = true;
            TouchPos = ((PointerEventData) data).position;
        });

        AddCallback(EventTriggerType.PointerUp, (data) =>
        {
            Dragging = false;
            Knob.anchoredPosition = Vector2.zero;
            Vertical = Horizontal = 0f;
        });

        AddCallback(EventTriggerType.Drag, (data) =>
        {
            if (Dragging) TouchPos = ((PointerEventData) data).position;
        });

        ///

        AddCallback(EventTriggerType.PointerUp, (data) => StopAllCoroutines(), ShootTrigger);

        AddCallback(EventTriggerType.PointerDown, (data) =>
        {
            if (Game.IsPlaying) StartCoroutine(ShootCoroutine());

            Game.game.RestartGameIfNeeded();
        }, ShootTrigger);
    }

    void Update()
    {
        if (!Dragging) return;

        Vector2 pos = (Vector2) Game.Camera.ScreenToViewportPoint(TouchPos) * Game.game.GamePlaceholder.rect.size;

        pos -= transform.anchoredPosition;
        pos.x -= Game.game.GamePlaceholder.rect.width - transform.rect.width;
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
        while (Game.IsPlaying)
        {
            Game.game.Player.Shoot();
            yield return null;
        }
    }
}