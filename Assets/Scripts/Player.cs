using System.Collections;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    const string Horizontal = "Horizontal";
    const string Vertical = "Vertical";
    float LastShootTime = 0f;

    [HideInInspector]
    public OffScreen OffScreen = null;

    [HideInInspector]
    public new RectTransform transform = null;
    [HideInInspector]
    public TextMeshProUGUI ScoreText = null;
    [HideInInspector]
    public AudioSource ShootSound = null;

    void Awake()
    {
        transform = gameObject.transform as RectTransform;
        ScoreText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ShootSound = GetComponent<AudioSource>();

        OffScreen = GetComponent<OffScreen>();
    }

    void FixedUpdate()
    {
        if (!Game.IsPlaying) return;

        MobileJoystickInput();
        PCInput();
    }

    public void Shoot()
    {
        if (LastShootTime + .5f * Game.PlayerShootSpeed > Time.time) return;

        LastShootTime = Time.time;
        Game.game.Shoot();
    }

    void MovePlayer(float forward, float rotation)
    {
        transform.anchoredPosition += (Vector2) transform.up * forward * 50f * Game.PlayerSpeedMultiplier;
        transform.Rotate(0f, 0f, rotation * 6f);
    }

    void MobileJoystickInput() => MovePlayer(MobileJoystick.Vertical, -MobileJoystick.Horizontal);

    void MobileScreenInput()
    {
        var pos = Game.Camera.ScreenToViewportPoint(Input.mousePosition);

        pos.x *= Screen.width * 2f;
        pos.y *= Screen.height * 2f;
        pos -= new Vector3(Screen.width, Screen.height);

        var diff = (Vector2) pos - (Vector2) transform.anchoredPosition;
        if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) < 5f) return;

        float angle = Vector2.SignedAngle(diff, transform.up);

        float rotate = angle > 0 ? -1f : 1f;
        if (Mathf.Abs(angle) < 5f) rotate *= (Mathf.Abs(angle) / 5f);

        float move = 0f;
        if (Mathf.Abs(angle) < 90f) move = (90f - Mathf.Abs(angle)) / 90f;

        MovePlayer(move / 5f, rotate);
    }

    void PCInput()
    {
        MovePlayer(Input.GetAxis(Vertical) / 5f, -Input.GetAxis(Horizontal));

        if (Input.GetKey(KeyCode.Space)) Shoot();
    }
}