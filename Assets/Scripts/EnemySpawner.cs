using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public enum SpawnerState
{
    Disabled,
    Waiting,
    Spawning,
}

public class EnemySpawner : MonoBehaviour
{
    public float Interval;
    public PathData PathData;
    public WaveData WaveData;

    public GameObject WaveUiContainer;
    public TMP_Text WaveText;
    public TMP_Text AmtText;
    public TMP_Text FreqText;
    public Image TimerImage;

    public SpawnerState State;

    public Bounds Bounds { get { return GameManager.Instance.Bounds; } }
    public Enemy EnemyPrefab { get { return GameManager.Instance.EnemyPrefab; } }

    private WaveData.Entry _wave;
    private int _waveIndex;
    private float _waveDuration;

    private float _timer;
    private Queue<float> _spawnTimes;
    private float _spawnTimer;
    private float _startTimer;

    private float _waveScale;

    public void Initialize(PathData pathData, WaveData waveData)
    {
        PathData = pathData;
        WaveData = waveData;
        WaveUiContainer.transform.position =
            new Vector3(Bounds.Rect.xMin, Bounds.Rect.center.y) + Vector3.right * 0.2f;
        _waveScale = WaveUiContainer.transform.localScale.x;
        _spawnTimes = new Queue<float>();
    }

    public void StartWaves()
    {
        StartWave(0);
    }

    private void StartWave(int index)
    {
        _wave = WaveData.Waves[index];
        _waveIndex = index;
        _startTimer = _wave.Delay;

        WaveText.text = "WAVE " + (index + 1);
        AmtText.text = _wave.Amount.ToString();
        FreqText.text = string.Format("@ {0:F1}/s", _wave.Frequency);
        TimerImage.fillAmount = 1;

        _waveDuration = _wave.Amount / _wave.Frequency;
        var times = new List<float>();
        for (var i = 0; i < _wave.Amount; ++i)
        {
            times.Add(Random.Range(0, _waveDuration));
        }

        times.Sort();
        _spawnTimes = new Queue<float>(times);

        State = SpawnerState.Waiting;
        WaveUiContainer.transform.localScale = Vector3.zero;
        WaveUiContainer.transform.DOScale(_waveScale, 1)
            .SetEase(Ease.OutCubic);
    }

    private void StartSpawning()
    {
        State = SpawnerState.Spawning;
        _startTimer = 0;
        _spawnTimer = 0;

        WaveUiContainer.transform.DOScale(0, 1)
            .SetEase(Ease.InCubic);
    }

    private void FinishWave()
    {
        if (++_waveIndex < WaveData.Waves.Count)
            StartWave(_waveIndex);
    }

    private void Update()
    {
        if (State == SpawnerState.Waiting)
        {
            if ((_startTimer -= Time.deltaTime) < 0)
            {
                StartSpawning();
            }

            TimerImage.fillAmount = _startTimer / _wave.Delay;
        }
        else if (State == SpawnerState.Spawning)
        {
            _spawnTimer += Time.deltaTime;
            while (_spawnTimes.Count > 0 && _spawnTimes.Peek() < _spawnTimer)
            {
                _spawnTimes.Dequeue();
                Spawn();
            }

            if (_spawnTimes.Count == 0 && _spawnTimer > _waveDuration + 5)
            {
                FinishWave();
            }
        }
    }

    private void Spawn()
    {
        var enemy = Instantiate(EnemyPrefab);
        enemy.Initialize(PathData, _wave.Speed);
    }
}
