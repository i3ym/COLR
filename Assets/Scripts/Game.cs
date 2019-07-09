using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game game;
    public static Player Player;
    public static Camera Camera;
    public int Score { get => _score; set { _score = value; game.scoreText.text = value.ToString(); } }
    public static bool isPlaying = false;

    int _score = 0;
    public Dictionary<Rigidbody2D, Vector2> Movables = new Dictionary<Rigidbody2D, Vector2>();
    public Queue<Rigidbody2D> MeteorPool = new Queue<Rigidbody2D>();
    public Queue<Rigidbody2D> BulletPool = new Queue<Rigidbody2D>();
    Queue<ParticleSystem> DeathParticlePool = new Queue<ParticleSystem>();
    List<ParticleSystem> DeathParticles = new List<ParticleSystem>();

    [SerializeField]
    Player player = null;
    [SerializeField]
    public RectTransform gamePlaceholder = null;
    [SerializeField]
    GameObject BulletPrefab = null, meteorPrefab = null, DeathParticlePrefab = null;
    [SerializeField]
    TextMeshProUGUI scoreText = null;
    [SerializeField]
    RectTransform ParticlesParent = null, MeteorsParent = null;

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
    }

    void Start()
    {
        Destroy(GetComponent<Canvas>().worldCamera.gameObject);
        GetComponent<Canvas>().worldCamera = Camera = Camera.main;
        Player = player;

        MaxWorldPos = Camera.ViewportToWorldPoint(Vector3.one);

        StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());
    }

    [Conditional("DEBUG")]
    void Update() => Time.timeScale = TimeScale;

    void FixedUpdate()
    {
        if (isPlaying)
        {
            Score++;
            foreach (var movable in Movables.ToArray())
            {
                _pos = movable.Key.transform.position;
                if (Mathf.Abs(_pos.x) > MaxWorldPos.x + 2f || Mathf.Abs(_pos.y) > MaxWorldPos.y + 2f)
                {
                    Destroy(movable.Key.gameObject);
                    Movables.Remove(movable.Key);

                    continue;
                }

                movable.Key.MovePosition((Vector2) movable.Key.position + movable.Value);
            }
        }
    }

    public void GameOver()
    {
        if (!isPlaying) return;

        isPlaying = false;

        foreach (Rigidbody2D movable in Movables.Keys)
            if (movable)
            {
                PlayDeathParticle(movable.transform.position);
                Destroy(movable.gameObject);
            }

        RectTransform tr = scoreText.GetComponent<RectTransform>();
        tr.parent = gamePlaceholder;
        tr.rotation = Quaternion.identity;
        tr.anchorMax = tr.anchorMin = new Vector2(.5f, .5f);
        tr.anchoredPosition = Vector2.zero;

        PlayDeathParticle(player.transform.position);
        Destroy(player.gameObject);
    }

    public void PlayDeathParticle(Vector2 position)
    {
        if (Mathf.Abs(position.x) > MaxWorldPos.x || Mathf.Abs(position.y) > MaxWorldPos.y) return;

        ParticleSystem dp;

        if (DeathParticlePool.Count == 0)
        {
            dp = Instantiate(DeathParticlePrefab, game.gamePlaceholder).GetComponent<ParticleSystem>();
            dp.transform.SetParent(ParticlesParent, true);
        }
        else dp = DeathParticlePool.Dequeue();

        dp.gameObject.SetActive(true);

        dp.transform.parent = gamePlaceholder;
        dp.transform.position = position;

        dp.Clear();
        dp.Play();

        DeathParticles.Add(dp);
    }

    public void Shoot()
    {
        Rigidbody2D bullet;
        if (BulletPool.Count == 0) bullet = Instantiate(game.BulletPrefab).GetComponent<Rigidbody2D>();
        else bullet = BulletPool.Dequeue();

        bullet.gameObject.SetActive(true);

        bullet.transform.SetParent(game.gamePlaceholder, false);
        bullet.transform.position = Player.transform.position + Player.transform.up / 40f;
        Movables.Add(bullet, Player.transform.up / 20f);
    }

    public void RestartGameIfNeeded()
    {
        if (!isPlaying) SceneManager.LoadScene(0);
    }

    IEnumerator SpawnMeteorsCoroutine()
    {
        var wfi = new WaitUntil(() => isPlaying);

        while (true)
        {
            if (!isPlaying) yield return wfi;

            SpawnMeteor();
            yield return new WaitForSeconds(Random.value / 2f);
        }
    }

    public void SpawnMeteor()
    {
        if (!isPlaying) return;

        Vector2 spawnPos = new Vector2();
        if (Random.value >.5f)
        {
            spawnPos.x = (Random.value - .5f) * gamePlaceholder.rect.size.x;
            spawnPos.y = (Random.value >.5f ? -1f : 1f) * gamePlaceholder.rect.size.y / 2;
        }
        else
        {
            spawnPos.x = (Random.value >.5f ? -1f : 1f) * gamePlaceholder.rect.size.x / 2;
            spawnPos.y = (Random.value - .5f) * gamePlaceholder.rect.size.y;
        }

        Rigidbody2D meteor;
        if (MeteorPool.Count == 0)
        {
            meteor = Instantiate(meteorPrefab, game.gamePlaceholder).GetComponent<Rigidbody2D>();
            meteor.transform.SetParent(MeteorsParent, true);
        }
        else meteor = MeteorPool.Dequeue();

        meteor.gameObject.SetActive(true);
        (meteor.transform as RectTransform).anchoredPosition = spawnPos;

        Vector2 dirToPlayer = meteor.transform.position - Player.transform.position;
        dirToPlayer = dirToPlayer.normalized;
        dirToPlayer.x += Random.value * 4f - 2f;
        dirToPlayer.y += Random.value * 4f - 2f;

        Movables.Add(meteor, -dirToPlayer.normalized / 30f);
    }

    public async void SpawnMeteorNextFrame()
    {
        await Task.Delay((int) (Time.fixedDeltaTime * 1000f));
        SpawnMeteor();
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