using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public int Score { get => _score; set { _score = value; Player.ScoreText.text = value.ToString(); } }

    int _score = 0;
    int Highscore = 0;

    public List<Movable> Movables = new List<Movable>();
    Queue<ParticleSystem> DeathParticlePool = new Queue<ParticleSystem>();
    List<ParticleSystem> DeathParticles = new List<ParticleSystem>();

    [SerializeField]
    public Player Player;
    [SerializeField]
    GameObject PlayerPrefab = null, DeathParticlePrefab = null;
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

    Vector2 MaxCanvasPos;

    Vector4[] _Meteors = new Vector4[100];
    Color[] _MeteorColors = new Color[100];
    Vector4[] _Bullets = new Vector4[50];

    Vector2 _SpawnPos = new Vector2();

    Movable _Movable1, _Movable2;
    Movable[] _Movables;

    void Start()
    {
        game = this;
        Camera = Camera.main;
        MaxCanvasPos = GamePlaceholder.rect.max;

        LoadSettings();

        gameObject.SetActive(false);

        IsAlive = true;
        IsPaused = false;
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
        MoveMovables();
        RedrawMovables();
        CastColliders();
    }

    ///

    public void SaveSettings() =>
        File.WriteAllLines(Path.Combine(Application.persistentDataPath, "config.cfg"), new string[]
        {
            Highscore.ToString(), Prefs.Bloom.ToString(), Prefs.Chroma.ToString(), Prefs.Grain.ToString(), Prefs.Lens.ToString(), (Music.volume * 100f).ToString(), (Player.ShootSound.volume * 100f).ToString()
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

        StartCoroutine(SpawnMeteorsCoroutine());
        StartCoroutine(RemoveParticlesCoroutine());

        IsAlive = true;
        IsPaused = false;
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

        GameOverHighscoreText.text = "рекорд: " + Highscore.ToString();
        GameOverHighscoreText.gameObject.SetActive(true);

        PlayDeathParticle(Player.transform.position);
        Player.gameObject.SetActive(false);

        RedrawMovables();
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

    void MoveMovables()
    {
        foreach (var movable in Movables.ToArray())
        {
            if (Mathf.Abs(movable.Position.x) > MaxCanvasPos.x + 2f || Mathf.Abs(movable.Position.y) > MaxCanvasPos.y + 2f)
            {
                movable.Death();
                continue;
            }

            movable.Position += movable.Direction;
        }
    }

    bool Overlaps(Movable mov1, Movable mov2) => Overlaps(mov1, mov2.Position, new Vector2(mov2.SizeX, mov2.SizeY));

    bool Overlaps(Movable mov1, Vector2 center2, Vector2 size2)
    {
        float minx1 = mov1.Position.x - mov1.SizeX / 2f;
        float miny1 = mov1.Position.y - mov1.SizeY / 2f;
        float minx2 = center2.x - size2.x / 2f;
        float miny2 = center2.y - size2.y / 2f;

        float maxx1 = mov1.Position.x + mov1.SizeX / 2f;
        float maxy1 = mov1.Position.y + mov1.SizeY / 2f;
        float maxx2 = center2.x + size2.x / 2f;
        float maxy2 = center2.y + size2.y / 2f;

        return maxx2 >= minx1 && minx2 <= maxx1 && maxy2 >= miny1 && miny2 <= maxy1;
    }

    void CastColliders()
    {
        _Movables = Movables.ToArray();

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

    void RedrawMovables()
    {
        if (Movables.Count == 0)
        {
            MovablesRenderer.material.SetVector("_Counts", Vector2.zero);
            return;
        }

        var meteors = Movables.Where(x => x is Meteor).Cast<Meteor>();
        var bullets = Movables.Where(x => x is Bullet);
        int meteorCount = meteors.Count();

        MovablesRenderer.material.SetVector("_Counts", new Vector2(meteorCount, Movables.Count - meteorCount));

        Vector4 Convert(Movable x) => Convertv(x.Position);
        Vector4 Convertv(Vector2 x) => (Vector4) ((x + MaxCanvasPos) / (MaxCanvasPos * 2f) * new Vector2(Screen.width, Screen.height));

        MovablesRenderer.material.SetVector("_Sizes", Convertv(new Vector2(new Meteor().SizeX, new Bullet().SizeX) / 2f - MaxCanvasPos));

        if (meteorCount != 0)
        {
            meteors.Select(x => Convert(x)).ToArray().CopyTo(_Meteors, 0);
            MovablesRenderer.material.SetVectorArray("_Meteors", _Meteors);

            meteors.Select(x => x.Effect.Color).ToArray().CopyTo(_MeteorColors, 0);
            MovablesRenderer.material.SetColorArray("_MeteorColors", _MeteorColors);
        }

        if (bullets.Count() != 0)
        {
            bullets.Select(x => Convert(x)).ToArray().CopyTo(_Bullets, 0);
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
}