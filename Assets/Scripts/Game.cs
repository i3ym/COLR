using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game game;
    public static Player Player;
    public static Camera Camera;
    public static bool IsPlaying;
    public static float PlayerSpeedMultiplier, PlayerShootSpeed;
    public int Score { get => _score; set { _score = value; game.ScoreText.text = value.ToString(); } }

    int _score = 0;
    public List<Movable> Movables = new List<Movable>();
    public Queue<Meteor> MeteorPool = new Queue<Meteor>();
    public Queue<Bullet> BulletPool = new Queue<Bullet>();
    Queue<ParticleSystem> DeathParticlePool = new Queue<ParticleSystem>();
    List<ParticleSystem> DeathParticles = new List<ParticleSystem>();

    Dictionary<MeteorEffectType, Material> MeteorMaterials = new Dictionary<MeteorEffectType, Material>();

    [SerializeField]
    Player player = null;
    [SerializeField]
    public RectTransform GamePlaceholder = null;
    [SerializeField]
    GameObject BulletPrefab = null, MeteorPrefab = null, DeathParticlePrefab = null;
    [SerializeField]
    TextMeshProUGUI ScoreText = null;
    [SerializeField]
    RectTransform ParticlesParent = null, MeteorsParent = null, BulletsParent = null;

    public float TimeScale = 1f;
    Vector2 MaxWorldPos;

    Vector2 _pos;

    void Awake()
    {
        game = this;

        Movables.Clear();
        MeteorPool.Clear();
        DeathParticlePool.Clear();
        DeathParticles.Clear();

        Score = 0;
        PlayerSpeedMultiplier = 1f;
        PlayerShootSpeed = 1f;
    }

    void Start()
    {
        Camera = Camera.main;
        Player = player;
        MaxWorldPos = Camera.ViewportToWorldPoint(Vector3.one);

        StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());

        Prefs.UpdateCameraPrefs(Camera);

        IsPlaying = true;
    }

    void Update()
    {
#if DEBUG
        Time.timeScale = TimeScale;
#endif

        if (Input.GetMouseButtonDown(0)) Game.game.RestartGameIfNeeded();
    }

    void FixedUpdate()
    {
        if (!IsPlaying) return;

        Score++;
        MoveMovables();
        CastColliders();
    }

    public void GameOver()
    {
        if (!IsPlaying) return;

        IsPlaying = false;

        foreach (var movable in Movables)
            if (movable && movable.gameObject.activeSelf)
            {
                PlayDeathParticle(movable.transform.position);
                Destroy(movable.gameObject);
            }

        RectTransform tr = ScoreText.GetComponent<RectTransform>();
        tr.parent = GamePlaceholder;
        tr.rotation = Quaternion.identity;
        tr.anchorMax = tr.anchorMin = new Vector2(.5f, .5f);
        tr.anchoredPosition = Vector2.zero;

        PlayDeathParticle(player.transform.position);
        Destroy(player.gameObject);
    }

    public void RestartGameIfNeeded()
    {
        if (!IsPlaying) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void MoveMovables()
    {
        foreach (var movable in Movables.ToArray())
        {
            _pos = movable.transform.position;

            if (Mathf.Abs(_pos.x) > MaxWorldPos.x + 2f || Mathf.Abs(_pos.y) > MaxWorldPos.y + 2f)
            {
                movable.Death();
                continue;
            }

            movable.transform.position += movable.Direction;
        }
    }

    void CastColliders()
    {
        Rect rect1, rect2;
        Movable movable, movable2;
        var movables = Movables.ToArray();

        for (int i = 0; i < movables.Length; i++)
        {
            movable = movables[i];

            if (!movable.gameObject.activeSelf) continue;

            rect1 = movable.transform.rect;
            rect1.center = movable.transform.anchoredPosition;

            if (movable is Meteor)
            {
                rect2 = Player.transform.rect;
                rect2.center = Player.transform.anchoredPosition;

                if (rect1.Overlaps(rect2))
                {
                    (movable as Meteor).Effect.PlayEffect();
                    movable.Death();
                    return;
                }
            }

            for (int j = 0; j < movables.Length; j++)
            {
                movable2 = movables[i];

                if (movable == movable2 || !movable2.gameObject.activeSelf) continue;

                rect2 = movable2.transform.rect;
                rect2.center = movable2.transform.anchoredPosition;

                if (rect1.Overlaps(rect2))
                {
                    movable.Death();
                    movable2.Death();
                }
            }
        }
    }

    public void PlayDeathParticle(Vector2 position) => PlayDeathParticle(position, Color.white);

    public void PlayDeathParticle(Vector2 position, Color color)
    {
        if (Mathf.Abs(position.x) > MaxWorldPos.x || Mathf.Abs(position.y) > MaxWorldPos.y) return;

        ParticleSystem dp;

        if (DeathParticlePool.Count == 0)
        {
            dp = Instantiate(DeathParticlePrefab, game.GamePlaceholder).GetComponent<ParticleSystem>();
            dp.transform.SetParent(ParticlesParent, true);
        }
        else dp = DeathParticlePool.Dequeue();

        dp.gameObject.SetActive(true);

        dp.transform.parent = GamePlaceholder;
        dp.transform.position = position;

        var main = dp.main;
        main.startColor = color;

        dp.Clear();
        dp.Play();

        DeathParticles.Add(dp);
    }

    public void Shoot()
    {
        Bullet bullet;
        if (BulletPool.Count == 0)
        {
            bullet = Instantiate(game.BulletPrefab).GetComponent<Bullet>();
            bullet.transform.SetParent(BulletsParent, true);
            bullet.transform.localScale = Vector3.one;
        }
        else bullet = BulletPool.Dequeue();

        bullet.gameObject.SetActive(true);
        bullet.transform.position = Player.transform.position + Player.transform.up / 40f;

        bullet.Direction = Player.transform.up / 20f;
        Movables.Add(bullet);
    }

    IEnumerator SpawnMeteorsCoroutine()
    {
        var wfi = new WaitUntil(() => IsPlaying);

        while (true)
        {
            if (!IsPlaying) yield return wfi;

            SpawnMeteor();
            yield return new WaitForSeconds(Random.value / 2f);
        }
    }

    public async void SpawnMeteorNextFrame()
    {
        await Task.Delay((int) (Time.fixedDeltaTime * 1000f));
        SpawnMeteor();
    }

    public void SpawnMeteor()
    {
        if (!IsPlaying) return;

        Vector2 spawnPos = new Vector2();
        if (Random.value >.5f)
        {
            spawnPos.x = (Random.value - .5f) * GamePlaceholder.rect.size.x;
            spawnPos.y = (Random.value >.5f ? -1f : 1f) * GamePlaceholder.rect.size.y / 2 - 10f;
        }
        else
        {
            spawnPos.x = (Random.value >.5f ? -1f : 1f) * GamePlaceholder.rect.size.x / 2 - 10f;
            spawnPos.y = (Random.value - .5f) * GamePlaceholder.rect.size.y;
        }

        Meteor meteor;
        if (MeteorPool.Count == 0)
        {
            meteor = Instantiate(MeteorPrefab, game.GamePlaceholder).GetComponent<Meteor>();
            meteor.transform.SetParent(MeteorsParent, true);
        }
        else meteor = MeteorPool.Dequeue();

        meteor.gameObject.SetActive(true);
        meteor.transform.anchoredPosition = spawnPos;

        Vector2 dirToPlayer = meteor.transform.position - Player.transform.position;
        dirToPlayer = dirToPlayer.normalized;
        dirToPlayer.x += Random.value * 4f - 2f;
        dirToPlayer.y += Random.value * 4f - 2f;

        /// assign a random effect

        float randomEffect = Random.value;

        if (randomEffect >.98) meteor.EffectType = MeteorEffectType.Speedup;
        else if (randomEffect >.96) meteor.EffectType = MeteorEffectType.Slowdown;
        else if (randomEffect >.94) meteor.EffectType = MeteorEffectType.FasterShoot;
        else if (randomEffect >.92) meteor.EffectType = MeteorEffectType.SlowerShoot;

        /// assign a random effect ///

        meteor.GetComponent<RawImage>().color = meteor.Effect.Color;

        meteor.Direction = -dirToPlayer.normalized / 30f;
        Movables.Add(meteor);
    }

    IEnumerator RemoveParticlesCoroutine()
    {
        var wff = new WaitForFixedUpdate();

        while (true)
        {
            while (DeathParticles.Count == 0) yield return wff;

            foreach (var particle in DeathParticles.ToArray())
                if (!particle.isPlaying)
                {
                    DeathParticlePool.Enqueue(particle);
                    DeathParticles.Remove(particle);
                }

            yield return wff;
        }
    }
}