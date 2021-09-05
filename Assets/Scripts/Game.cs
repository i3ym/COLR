using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static Game game;
    public static Camera Camera;
    public static Prefs Prefs;
    public static bool IsAlive { get; private set; }
    public static bool IsPaused { get; private set; }
    public static bool IsPlaying { get => IsAlive && !IsPaused; }
    public static float PlayerSpeedMultiplier, PlayerShootSpeed;
    public static List<LocalizedText> LocalizedTexts = new List<LocalizedText>();
    public int Score { get => _score; set { _score = value; Player.ScoreText.text = value.ToString(); } }

    int _score = 0;
    int Highscore = 0;

    public MovablesList Movables = new MovablesList();
    Queue<ParticleSystem> DeathParticlePool = new Queue<ParticleSystem>();
    List<ParticleSystem> DeathParticles = new List<ParticleSystem>();
    Vector2 MaxCanvasPos;

    [SerializeField]
    public Player Player;
    [SerializeField]
    GameObject DeathParticlePrefab = null;
    [SerializeField]
    public RectTransform GamePlaceholder = null;
    [SerializeField]
    RectTransform ParticlesParent = null;
    [SerializeField]
    TextMeshProUGUI GameOverScoreText = null, GameOverHighscoreText = null;
    [SerializeField]
    MainMenu MainMenu = null;
    [SerializeField]
    public AudioSource Music = null;

    Vector4[] _Meteors = new Vector4[100];
    Color[] _MeteorColors = new Color[100];
    Vector4[] _Bullets = new Vector4[50];

    Vector2 _SpawnPos = new Vector2();

    Movable _Movable1, _Movable2;
    Movable[] _Movables;
    int _LastMeteorsCount, _LastBulletsCount;

    void Awake() => LocalizedTexts.Clear();

    void Start()
    {
        game = this;
        Camera = Camera.main;
        MaxCanvasPos = GamePlaceholder.rect.max;

        LoadSettings();

        gameObject.SetActive(false);

        IsAlive = true;
        IsPaused = false;

        SetLanguage(Prefs.Lang);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPlaying) PauseGame();
            else if (IsPaused) MainMenu.Continue();
        }

        if (Input.GetMouseButtonDown(0)) Game.game.RestartGameIfNeeded();
    }

    void FixedUpdate()
    {
        if (!Game.IsPlaying) return;

        Score++;
        _Movables = Movables.ToArray();

        MoveMovables();
    }

    ///

    public void SaveSettings() =>
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "config.cfg"), JsonUtility.ToJson(Prefs));

    public void LoadSettings()
    {
        Prefs = new Prefs();

        if (!File.Exists(Path.Combine(Application.persistentDataPath, "config.cfg"))) return;

        try
        {
            Prefs = JsonUtility.FromJson<Prefs>(File.ReadAllText(Path.Combine(Application.persistentDataPath, "config.cfg")));
        }
        catch { }

        Prefs.Update();
    }

    ///

    public void StartGame()
    {
        Player.transform.localPosition = Vector2.zero;
        Player.transform.rotation = Quaternion.identity;
        Player.gameObject.SetActive(true);
        GameOverScoreText.gameObject.SetActive(false);
        GameOverHighscoreText.gameObject.SetActive(false);

        Movables.Clear();
        DeathParticlePool.Clear();
        DeathParticles.Clear();

        Score = 0;
        PlayerSpeedMultiplier = 1f;
        PlayerShootSpeed = 1f;

        // Movables.Add(new Meteor() { Position = new Vector2(100f, 100f), Direction = Vector2.zero });
        StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());

        IsAlive = true;
        IsPaused = false;

        Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = true;

        // StartCoroutine(PerformanceTestDeath());
    }

    IEnumerator PerformanceTestDeath()
    {
        yield return new WaitForSeconds(2);
        GameOver();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;

        MainMenu.Pause();
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void GameOver()
    {
        if (!IsAlive) return;

        IsAlive = false;

        foreach (var movable in Movables)
            if (movable)
                Destroy(movable.gameObject);

        Movables.Clear();

        if (Highscore < Score)
        {
            Highscore = Score;
            SaveSettings();
        }

        GameOverScoreText.text = Score.ToString();
        GameOverScoreText.gameObject.SetActive(true);

        GameOverHighscoreText.text = LocalizedText.GetTranslation("text.Highscore", Prefs.Lang) + ": " + Highscore.ToString();
        GameOverHighscoreText.gameObject.SetActive(true);

        PlayDeathParticle(Player.transform.position);
        Player.gameObject.SetActive(false);

        Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = true;
    }

    void RestartGame()
    {
        StopAllCoroutines();
        StartGame();
    }

    public void RestartGameIfNeeded()
    {
        if (!IsAlive) RestartGame();
    }

    ///

    [MethodImpl(256)]
    void MoveMovables()
    {
        foreach (var movable in _Movables)
        {
            if (Mathf.Abs(movable.transform.anchoredPosition.x) > MaxCanvasPos.x + 2f || Mathf.Abs(movable.transform.anchoredPosition.y) > MaxCanvasPos.y + 2f)
            {
                movable.Death();
                continue;
            }

            movable.transform.anchoredPosition += movable.Direction;
        }
    }

    ///

    public void PlayDeathParticle(Vector2 canvasPosition) => PlayDeathParticle(canvasPosition, Color.white);

    public void PlayDeathParticle(Vector2 canvasPosition, Color color)
    {
        if (!Prefs.Particles) return;

        if (Mathf.Abs(canvasPosition.x) > MaxCanvasPos.x || Mathf.Abs(canvasPosition.y) > MaxCanvasPos.y) return;

        ParticleSystem dp;

        if (DeathParticlePool.Count == 0)
        {
            dp = Instantiate(DeathParticlePrefab, game.GamePlaceholder).AddComponent<RectTransform>().GetComponent<ParticleSystem>();
            dp.transform.SetParent(ParticlesParent, true);
        }
        else dp = DeathParticlePool.Dequeue();

        dp.gameObject.SetActive(true);
        (dp.transform as RectTransform).anchoredPosition = canvasPosition;

        var main = dp.main;
        main.startColor = color;

        dp.Clear();
        dp.Play();

        DeathParticles.Add(dp);
    }

    ///

    public void Shoot()
    {
        var bulletgo = new GameObject("bullet", typeof(RectTransform), typeof(RawImage), typeof(BoxCollider2D));
        var bullet = bulletgo.AddComponent<Bullet>();

        bulletgo.GetComponent<BoxCollider2D>().size = bullet.transform.sizeDelta;
        bulletgo.GetComponent<BoxCollider2D>().isTrigger = true;

        bullet.transform.SetParent(transform, false);
        bullet.transform.anchoredPosition = Player.transform.anchoredPosition;
        bullet.Direction = Player.transform.up * 5f;
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

        if (IsPlaying) SpawnMeteor();
    }

    public void SpawnMeteor()
    {
        if (Movables.Count(x => x is Meteor) > 98) return; //TODO 

        int side = Mathf.RoundToInt(Random.value / .25f);
        float pos = Random.value * 2f - 1f;

        if (side == 0)
        {
            _SpawnPos.y = MaxCanvasPos.y;
            _SpawnPos.x = MaxCanvasPos.x * pos;
        }
        else if (side == 1)
        {
            _SpawnPos.y = -MaxCanvasPos.y;
            _SpawnPos.x = MaxCanvasPos.x * pos;
        }
        else if (side == 2)
        {
            _SpawnPos.y = MaxCanvasPos.y * pos;
            _SpawnPos.x = MaxCanvasPos.x;
        }
        else if (side == 3)
        {
            _SpawnPos.y = MaxCanvasPos.y * pos;
            _SpawnPos.x = -MaxCanvasPos.x;
        }

        var meteorgo = new GameObject("meteor", typeof(RectTransform), typeof(RawImage), typeof(BoxCollider2D));
        var meteor = meteorgo.AddComponent<Meteor>();

        meteorgo.GetComponent<BoxCollider2D>().size = meteor.transform.sizeDelta;
        meteorgo.GetComponent<BoxCollider2D>().isTrigger = true;

        meteor.transform.SetParent(transform, false);
        meteor.transform.anchoredPosition = _SpawnPos;
        meteor.Direction = new Vector2(Random.value - .5f, Random.value - .5f).normalized * 5f;

        Movables.Add(meteor);

        /// assign a random effect

        float randomEffect = Random.value;

        if (randomEffect >.98) meteor.EffectType = MeteorEffectType.Speedup;
        else if (randomEffect >.96) meteor.EffectType = MeteorEffectType.Slowdown;
        else if (randomEffect >.94) meteor.EffectType = MeteorEffectType.FasterShoot;
        else if (randomEffect >.92) meteor.EffectType = MeteorEffectType.SlowerShoot;
        else meteor.EffectType = MeteorEffectType.None;

        /// assign a random effect ///

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

    ///

    public static void SetLanguage(Language lang)
    {
        Prefs.Lang = lang;
        foreach (var localizer in LocalizedTexts) localizer.SetLanguage(lang);
    }
}