using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    const string Horizontal = "Horizontal";
    const string Vertical = "Vertical";
    bool DoShoot = false;

    [HideInInspector]
    public new RectTransform transform;
    [SerializeField]
    MobileJoystick Joystick = null;

    void Awake()
    {
        if (name.Contains("Clone")) Destroy(this);

#if DEBUG
        // gameObject.GetComponent<PolygonCollider2D>().enabled = false;
#endif
    }

    void Start()
    {
        transform = gameObject.transform as RectTransform;

        StartCoroutine(ShootCoroutine());
    }

    void FixedUpdate()
    {
        MobileJoystickInput();
        PCInput();
    }

    IEnumerator ShootCoroutine()
    {
        var wfi = new WaitUntil(() => DoShoot);

        while (true)
        {
            DoShoot = false;
            yield return wfi;

            Game.game.Shoot();
            yield return new WaitForSeconds(.5f * Game.PlayerShootSpeed);
        }
    }

    public void Shoot() => DoShoot = true;

    void MovePlayer(float forward, float rotation)
    {
        transform.anchoredPosition += (Vector2) transform.up * forward * 50f * Game.PlayerSpeedMultiplier;
        transform.Rotate(0f, 0f, rotation * 6f);
    }

    void MobileJoystickInput() => MovePlayer(Joystick.Vertical, -Joystick.Horizontal);

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
        MovePlayer(Input.GetAxis(Vertical), -Input.GetAxis(Horizontal));

        if (Input.GetKey(KeyCode.Space)) DoShoot = true;
    }
}