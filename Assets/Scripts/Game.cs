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
    [SerializeField]
    RawImage MovablesRenderer = null;

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

        var sw = System.Diagnostics.Stopwatch.StartNew();

        Score++;
        _Movables = Movables.ToArray();

        var arr = sw.ElapsedTicks;

        MoveMovables();

        var moved = sw.ElapsedTicks - arr;

        CastColliders();

        var castd = sw.ElapsedTicks - arr - moved;

        RedrawMovables();

        var red = sw.ElapsedTicks - arr - moved - castd;

        sw.Stop();
        Debug.Log(arr + "/" + moved + "/" + castd + "/" + red);
    }

    ///

    public void SaveSettings() =>
        File.WriteAllLines(Path.Combine(Application.persistentDataPath, "config.cfg"), new string[]
        {
            Highscore.ToString(), Prefs.Bloom.ToString(), Prefs.Chroma.ToString(), Prefs.Grain.ToString(), Prefs.Lens.ToString(),
                (Music.volume * 100f).ToString(), (Player.ShootSound.volume * 100f).ToString(), ((int) Prefs.Lang).ToString()
        });

    public void LoadSettings()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "config.cfg"))) return;

        var cfg = File.ReadAllLines(Path.Combine(Application.persistentDataPath, "config.cfg"));

        try
        {
            Highscore = int.Parse(cfg[0]);
            Prefs.Bloom = bool.Parse(cfg[1]);
            Prefs.Chroma = bool.Parse(cfg[2]);
            Prefs.Grain = bool.Parse(cfg[3]);
            Prefs.Lens = bool.Parse(cfg[4]);

            Music.volume = float.Parse(cfg[5]) / 100f;
            Player.ShootSound.volume = float.Parse(cfg[6]) / 100f;

            Prefs.Lang = (Language) int.Parse(cfg[7]);
            SetLanguage(Prefs.Lang);
        }
        catch { }

        Prefs.UpdateCameraPrefs(Camera);
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

        Movables.Add(new Meteor() { Position = new Vector2(100f, 100f), Direction = Vector2.zero });
        //StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());

        IsAlive = true;
        IsPaused = false;

        MovablesRenderer.material.SetVector("_Sizes", VectorToScreenPos(new Vector2(new Meteor().SizeX, new Bullet().SizeX) / 2f - MaxCanvasPos));
        Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = true;

        StartCoroutine(A());
    }

    IEnumerator A()
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

        RedrawMovables();

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
            if (Mathf.Abs(movable.Position.x) > MaxCanvasPos.x + 2f || Mathf.Abs(movable.Position.y) > MaxCanvasPos.y + 2f)
            {
                movable.Death();
                continue;
            }

            movable.Position += movable.Direction;
        }
    }

    ///

    [MethodImpl(256)]
    bool Overlaps(Movable mov1, Movable mov2) => Overlaps(mov1.Position, mov1.SizeX, mov1.SizeY, mov2.Position, mov2.SizeX, mov2.SizeY);

    [MethodImpl(256)]
    bool Overlaps(Movable mov1, Vector2 center2, Vector2 size2) => Overlaps(mov1.Position, mov1.SizeX, mov1.SizeY, center2, size2.x, size2.y);

    [MethodImpl(256)]
    bool Overlaps(Vector2 center1, float size1X, float size1Y, Vector2 center2, float size2X, float size2Y)
    {
        var diff = center1 - center2;
        return (Mathf.Abs(diff.x) < (size2X + size1X) / 2f) && (Mathf.Abs(diff.y) < (size2Y + size1Y) / 2f);
    }

    [MethodImpl(256)]
    void CastColliders()
    {
        for (int i = 0; i < _Movables.Length; i++)
        {
            _Movable1 = _Movables[i];

            if (!_Movable1.IsAlive) continue;

            if (_Movable1 is Meteor)
            {
                if (Overlaps(_Movable1, Player.transform.anchoredPosition, Player.transform.rect.size) ||
                    Overlaps(_Movable1, Player.OffScreen.shadowGO.anchoredPosition, Player.OffScreen.shadowGO.rect.size))
                {
                    (_Movable1 as Meteor).Effect.PlayEffect();
                    _Movable1.Death();

                    return;
                }
            }

            for (int j = 0; j < _Movables.Length; j++)
            {
                _Movable2 = _Movables[j];

                if (!_Movable2.IsAlive || _Movable1 == _Movable2) continue;

                if (Overlaps(_Movable1, _Movable2))
                {
                    if (_Movable2 is Bullet && (_Movable1 as Meteor).EffectType != MeteorEffectType.None) (_Movable1 as Meteor).Effect.PlayEffect();

                    _Movable1.Death();
                    _Movable2.Death();
                }
            }
        }
    }

    ///

    Vector4 MovableToScreenPos(Movable x) => VectorToScreenPos(x.Position);

    Vector4 VectorToScreenPos(Vector2 x) => (Vector4) ((x + MaxCanvasPos) / (MaxCanvasPos * 2f) * new Vector2(Screen.width, Screen.height));

    void RedrawMovables()
    {
        var meteorCount = Movables.Meteors.Count;
        var bulletCount = Movables.Meteors.Count;

        if (Movables.Count == 0)
        {
            if (meteorCount != _LastMeteorsCount || bulletCount != _LastBulletsCount)
            {
                _LastMeteorsCount = meteorCount;
                _LastBulletsCount = bulletCount;

                MovablesRenderer.material.SetVector("_Counts", new Vector2(meteorCount, bulletCount));
            }
            return;
        }

        if (meteorCount != _LastMeteorsCount || bulletCount != _LastBulletsCount)
        {
            _LastMeteorsCount = meteorCount;
            _LastBulletsCount = bulletCount;

            MovablesRenderer.material.SetVector("_Counts", new Vector2(meteorCount, bulletCount));
        }

        if (meteorCount != 0)
        {
            Movables.Meteors.Select(x => MovableToScreenPos(x)).ToArray().CopyTo(_Meteors, 0);
            MovablesRenderer.material.SetVectorArray("_Meteors", _Meteors);

            Movables.Meteors.Select(x => x.Effect.Color).ToArray().CopyTo(_MeteorColors, 0);
            MovablesRenderer.material.SetColorArray("_MeteorColors", _MeteorColors);
        }

        if (bulletCount != 0)
        {
            Movables.Bullets.Select(x => MovableToScreenPos(x)).ToArray().CopyTo(_Bullets, 0);
            MovablesRenderer.material.SetVectorArray("_Bullets", _Bullets);
        }
    }

    ///

    public void PlayDeathParticle(Vector2 canvasPosition) => PlayDeathParticle(canvasPosition, Color.white);

    public void PlayDeathParticle(Vector2 canvasPosition, Color color)
    {
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

    public void Shoot() =>
        Movables.Add(new Bullet() { Position = Player.transform.anchoredPosition, Direction = Player.transform.up * 5f });

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

        Meteor meteor = new Meteor() { Position = _SpawnPos, Direction = new Vector2(Random.value - .5f, Random.value - .5f).normalized * 5f };

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