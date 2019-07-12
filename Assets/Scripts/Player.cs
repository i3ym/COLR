using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static bool isFluidControls = false;
    const string Horizontal = "Horizontal";
    const string Vertical = "Vertical";
    new Rigidbody2D rigidbody;
    new RectTransform transform;
    bool DoShoot = false;

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
        Physics2D.IgnoreLayerCollision(10, 11);

        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.isKinematic = !isFluidControls;

        transform = GetComponent<RectTransform>();

        StartCoroutine(ShootCoroutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Game.game.RestartGameIfNeeded();
    }

    void FixedUpdate()
    {
        MobileJoystickInput();
        // PCMovement();
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
        if (!isFluidControls)
        {
            transform.anchoredPosition += (Vector2) transform.up * forward * 50f * Game.PlayerSpeedMultiplier;
            rigidbody.MovePosition(transform.position);
            rigidbody.rotation += rotation * 6f;
        }
        else
        {
            rigidbody.AddForce(transform.up * forward * 2f * Game.PlayerSpeedMultiplier);
            rigidbody.AddTorque(rotation / 6f);
        }
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

        if (isFluidControls) MovePlayer(move / 5f, rotate);
        else MovePlayer(move / 5f, rotate);
    }

    void PCInput()
    {
        if (isFluidControls) MovePlayer(Input.GetAxis(Vertical), -Input.GetAxis(Horizontal));
        else MovePlayer(Input.GetAxis(Vertical), -Input.GetAxis(Horizontal));

        if (Input.GetKey(KeyCode.Space)) DoShoot = true;
    }
}