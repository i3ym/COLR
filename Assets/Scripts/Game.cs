using System.Collections;
using System.Collections.Generic;
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
    public static Dictionary<Rigidbody2D, Vector2> Bullets = new Dictionary<Rigidbody2D, Vector2>(); //go, direction
    static int _score = 0;
    public static int Score { get => _score; set { _score = value; game.scoreText.text = value.ToString(); } }
    public static bool isPlaying = true;
    public static BannerView Banner;

    [SerializeField]
    Player player = null;
    [SerializeField]
    public RectTransform gamePlaceholder = null;
    [SerializeField]
    GameObject BulletPrefab = null;
    [SerializeField]
    GameObject meteorPrefab = null;
    [SerializeField]
    TextMeshProUGUI scoreText = null;

    void Awake()
    {
        game = this;
    }
    void Start()
    {
        isPlaying = true;
        Camera = GetComponent<Camera>();
        Player = player;

        canvasSize = gamePlaceholder.rect.size;
        StartCoroutine(SpawnMeteorsCoroutine());

        CreateBanner();
    }

    void FixedUpdate()
    {
        if (isPlaying)
        {
            Score++;
            foreach (var bul in Bullets) bul.Key.MovePosition((Vector2) bul.Key.position + bul.Value);
        }
    }

    public void GameOver()
    {
        isPlaying = false;
        foreach (Rigidbody2D meteor in Bullets.Keys)
            if (meteor) DestroyObject(meteor.transform);
        Bullets.Clear();

        RectTransform tr = scoreText.GetComponent<RectTransform>(); //TODO meteor/bullet pool
        tr.parent = gamePlaceholder;
        tr.rotation = Quaternion.identity;
        tr.anchorMax = new Vector2(.5f, .5f);
        tr.anchorMin = new Vector2(.5f, .5f);
        tr.anchoredPosition = new Vector2();
        DestroyObject(Player.transform);
    }
    public void DestroyObject(Transform obj)
    {
        if (obj.childCount != 0)
        {
            Transform particle = obj.GetChild(0);
            particle.GetComponent<ParticleSystem>().Play();
            particle.parent = gamePlaceholder;
            Destroy(particle.gameObject, 2f);
        }
        Destroy(obj.gameObject);
    }

    void CreateBanner()
    {
        const string banner = "ca-app-pub-6291991022802883/2598907369";
        Banner = new BannerView(banner, AdSize.Banner, AdPosition.Top);
        AdRequest request = new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).AddTestDevice("B6994A4369B1E37C").Build();
        Banner.LoadAd(request);
    }

    public static GameObject Shoot()
    {
        var pos = Player.transform.position + Player.transform.up / 40f;

        GameObject bullet = Instantiate(game.BulletPrefab);
        bullet.transform.SetParent(game.gamePlaceholder, false);
        bullet.transform.position = pos;
        Bullets.Add(bullet.GetComponent<Rigidbody2D>(), Player.transform.up / 20f);
        return bullet;
    }
    public void RestartGameIfNeeded()
    {
        if (isPlaying) return;

        Score = 0;
        Bullets.Clear();
        SceneManager.LoadScene(0);
    }

    IEnumerator SpawnMeteorsCoroutine()
    {
        while (true)
        {
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

        GameObject meteor = Instantiate(meteorPrefab, game.gamePlaceholder);
        meteor.GetComponent<RectTransform>().anchoredPosition = spawnPos;

        Vector2 dirToPlayer = meteor.transform.position - Player.transform.position;
        dirToPlayer.x += Random.value * 4f - 2f;
        dirToPlayer.y += Random.value * 4f - 2f;
        Bullets.Add(meteor.GetComponent<Rigidbody2D>(), -dirToPlayer.normalized / 30f);
    }
}