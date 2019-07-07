﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game game;
    public static Player Player;
    public static Camera Camera;
    public static Vector2 canvasSize;
    public int Score { get => _score; set { _score = value; game.scoreText.text = value.ToString(); } }
    public static bool isPlaying;
    public static BannerView Banner;

    int _score = 0;
    public Dictionary<Rigidbody2D, Vector2> Movables = new Dictionary<Rigidbody2D, Vector2>();
    public Queue<GameObject> MeteorPool = new Queue<GameObject>();
    Queue<ParticleSystem> DeathParticlePool = new Queue<ParticleSystem>();
    Queue<ParticleSystem> DeathParticles = new Queue<ParticleSystem>();

    [SerializeField]
    Player player = null;
    [SerializeField]
    public RectTransform gamePlaceholder = null;
    [SerializeField]
    GameObject BulletPrefab = null, meteorPrefab = null, DeathParticlePrefab = null;
    [SerializeField]
    TextMeshProUGUI scoreText = null;

    public float TimeScale = 1f;

    void Awake()
    {
        game = this;

        Movables.Clear();
        MeteorPool.Clear();
        DeathParticlePool.Clear();
        DeathParticles.Clear();
        isPlaying = false;

        Score = 0;
    }
    void Start()
    {
        Camera = GetComponent<Camera>();
        Player = player;

        canvasSize = gamePlaceholder.rect.size;
        StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());

        CreateAdBanner();
    }

    [Conditional("DEBUG")]
    void Update() => Time.timeScale = TimeScale;
    void FixedUpdate()
    {
        if (isPlaying)
        {
            Score++;
            foreach (var movable in Movables) movable.Key.MovePosition((Vector2) movable.Key.position + movable.Value);
        }
    }

    void CreateAdBanner()
    {
        const string banner = "ca-app-pub-6291991022802883/2598907369";
        Banner = new BannerView(banner, AdSize.Banner, AdPosition.Top);
        AdRequest request = new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).AddTestDevice("B6994A4369B1E37C").Build();
        Banner.LoadAd(request);
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
        ParticleSystem dp;

        if (DeathParticlePool.Count == 0) dp = Instantiate(DeathParticlePrefab, game.gamePlaceholder).GetComponent<ParticleSystem>();
        else dp = DeathParticlePool.Dequeue();

        dp.gameObject.SetActive(true);

        dp.transform.parent = gamePlaceholder;
        dp.transform.position = position;
        dp.Play();

        DeathParticles.Enqueue(dp);
    }

    public GameObject Shoot()
    {
        // TODO bullet pool
        var pos = Player.transform.position + Player.transform.up / 40f;

        GameObject bullet = Instantiate(game.BulletPrefab);
        bullet.transform.SetParent(game.gamePlaceholder, false);
        bullet.transform.position = pos;
        Movables.Add(bullet.GetComponent<Rigidbody2D>(), Player.transform.up / 20f);
        return bullet;
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
            spawnPos.x = (Random.value - .5f) * canvasSize.x;
            spawnPos.y = (Random.value >.5f ? -1f : 1f) * canvasSize.y / 2;
        }
        else
        {
            spawnPos.x = (Random.value >.5f ? -1f : 1f) * canvasSize.x / 2;
            spawnPos.y = (Random.value - .5f) * canvasSize.y;
        }

        GameObject meteor;
        if (MeteorPool.Count == 0) meteor = Instantiate(meteorPrefab, game.gamePlaceholder);
        else meteor = MeteorPool.Dequeue();

        meteor.SetActive(true);
        (meteor.transform as RectTransform).anchoredPosition = spawnPos;

        Vector2 dirToPlayer = meteor.transform.position - Player.transform.position;
        dirToPlayer.x += Random.value * 4f - 2f;
        dirToPlayer.y += Random.value * 4f - 2f;

        Movables.Add(meteor.GetComponent<Rigidbody2D>(), -dirToPlayer.normalized / 30f);
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
            if (!DeathParticles.Peek().IsAlive()) DeathParticlePool.Enqueue(DeathParticles.Dequeue());

            yield return wff;
        }
    }
}