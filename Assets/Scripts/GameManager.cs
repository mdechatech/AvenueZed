using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Bounds Bounds;

    public Traffic Traffic;
    public Levels Levels;

    public Button[] TurretButtons;
    public Turret[] TurretPrefabs;
    public int[] TurretCosts;

    [Space]
    public Car CarPrefab;
    public Crossing CrossingPrefab;
    public Road RoadPrefab;
    public CarSpawner CarSpawnerPrefab;
    public Path PathPrefab;
    public Enemy EnemyPrefab;
    public GunTracer TracerPrefab;
    public GunTracer BombTracerPrefab;
    public BoxCollider2D TurretBlockerPrefab;
    public float TurretBlockerSize { get { return TurretBlockerPrefab.size.x; } }

    [Space]
    public GameObject ExplosionPrefab;

    public LayerMask CarLayer;
    public LayerMask CarStopLayer;
    public LayerMask ZombieLayer;
    public LayerMask BlockerLayer;

    public int CarsFerried;
    public float AverageTripTime;

    public int Money;

    public int CarGoal;
    public float TimeGoal;

    public Queue<float> TripTimes;
    public UiManager Ui;
    public int GhostIndex;
    public Turret TurretGhost;

    public int LevelIndex;
    public LevelData Level;

    public List<Crossing> Crossings;

    public bool IsBuilding;
    public bool IsWinning;

    public static GameManager Instance { get; private set; }

    public void NotifyTripDone(Car car)
    {
        TripTimes.Enqueue(car.TravelTime);
        if (TripTimes.Count > CarGoal)
            TripTimes.Dequeue();

        AverageTripTime = TripTimes.Count > 0
            ? TripTimes.Average()
            : -1;

        ++CarsFerried;
        CarsFerried = Mathf.Min(CarsFerried, CarGoal);
        ++Money;

        print(TimeGoal - AverageTripTime);
        if (CarsFerried >= CarGoal && TimeGoal - AverageTripTime > -0.05f)
        {
            Win();
        }
    }


    public void NotifyEnemyPassed()
    {
        foreach (var crossing in Crossings)
            crossing.Collide(false);
    }

    public void NotifyEnemyDied()
    {
        ++Money;
    }

    public void Win()
    {
        Time.timeScale = 0;
        Ui.WinScreen.SetActive(true);
    }

    public void NextLevel()
    {
        DoLevel(LevelIndex + 1);
    }

    public void DoLevel(int index)
    {
        Time.timeScale = 1;
        Ui.WinScreen.SetActive(false);

        CancelGhost();

        Level = Levels.Data[index];
        LevelIndex = index;
        Traffic = FindObjectOfType<Traffic>();
        Traffic.Initialize(Traffic.Generate(Level));

        Money = Level.StartMoney;
        CarGoal = Level.CarGoal;
        TimeGoal = Level.TimeGoal;

        var spawner = FindObjectOfType<EnemySpawner>();
        spawner.StartWaves();
    }

    private void TrySelect(int turretIndex)
    {
        if (IsWinning)
            return;

        if (TurretGhost && turretIndex == GhostIndex)
        {
            CancelGhost();
            return;
        }

        var cost = TurretCosts[turretIndex];
        if (cost > Money)
        {
            Ui.FlashMoney();
            return;
        }

        if (TurretGhost)
        {
            Destroy(TurretGhost.gameObject);
        }

        GhostIndex = turretIndex;
        TurretGhost = Instantiate(TurretPrefabs[turretIndex]);
        TurretGhost.PlaceSprite.color = Color.red;
        Update();
    }

    private void CancelGhost()
    {
        if (TurretGhost)
            Destroy(TurretGhost.gameObject);
        GhostIndex = -1;
    }

    private void PlaceGhost()
    {
        var cost = TurretCosts[GhostIndex];
        if (cost > Money)
        {
            Ui.FlashMoney();
            return;
        }

        if (TurretGhost.IsBlocked())
            return;

        Money -= cost;
        TurretGhost.IsActivated = true;
        TurretGhost.PlaceSprite.gameObject.SetActive(false);
        TurretGhost = null;
        GhostIndex = -1;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        TripTimes = new Queue<float>();
        Crossings = new List<Crossing>();

        Instance = this;

        for (var i = 0; i < TurretPrefabs.Length; ++i)
        {
            var i1 = i;
            var keycode = (KeyCode) ((int)KeyCode.Alpha1 + i);
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(keycode))
                .Subscribe(_ => TrySelect(i1));
        }
    }

    private void Start()
    {
        Ui = FindObjectOfType<UiManager>();
        Ui.NextLevelButton.onClick.AddListener(NextLevel);

        for (int i = 0; i < TurretButtons.Length; ++i)
        {
            var i1 = i;
            TurretButtons[i].onClick.AddListener(() => TrySelect(i1));
        }

        DoLevel(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            DoLevel(LevelIndex + 1);

        if (!IsWinning && Input.GetKeyDown(KeyCode.R))
            DoLevel(LevelIndex);

        if (TurretGhost)
        {
            var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TurretGhost.transform.position = new Vector3(mouse.x, mouse.y, 0);

            if (TurretGhost.IsBlocked())
                TurretGhost.PlaceSprite.color = Color.red;
            else
                TurretGhost.PlaceSprite.color = Color.cyan;

            if (Input.GetMouseButtonDown(0))
                PlaceGhost();
        }
    }
}
