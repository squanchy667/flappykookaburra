using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private DifficultyConfig _difficultyConfig;
    [SerializeField] private ObstaclePair _obstaclePairPrefab;
    [SerializeField] private int _poolSize = 5;
    [SerializeField] private float _gracePeriod = 2f;

    private ObjectPool<ObstaclePair> _pool;
    private float _spawnTimer;
    private bool _spawning;
    private readonly List<ObstaclePair> _activeObstacles = new List<ObstaclePair>();

    private void Awake()
    {
        if (_obstaclePairPrefab == null)
        {
            Debug.LogError("[ObstacleSpawner] _obstaclePairPrefab is null! Run Tools > Game > Regenerate Prefabs to fix.");
            enabled = false;
            return;
        }

        Transform poolParent = new GameObject("ObstaclePool").transform;
        poolParent.SetParent(transform);
        _pool = new ObjectPool<ObstaclePair>(_obstaclePairPrefab, poolParent);
        _pool.PreWarm(_poolSize);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        if (!_spawning) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnObstacle();
            _spawnTimer = GetCurrentSpawnInterval();
        }

        ReturnOffScreenObstacles();
    }

    private void SpawnObstacle()
    {
        ObstaclePair pair = _pool.Get();

        float padding = GetCurrentGapYPadding();
        float minY = WorldBounds.ScreenBottom + padding;
        float maxY = WorldBounds.ScreenTop - padding;
        float gapCenterY = Random.Range(minY, maxY);
        float gapSize = GetCurrentGapSize();
        float speed = GetCurrentScrollSpeed();

        // Apply per-obstacle speed jitter
        float variance = GetCurrentSpeedVariance();
        if (variance > 0f)
            speed *= 1f + Random.Range(-variance, variance);

        // Get oscillation parameters
        float oscAmplitude = GetCurrentOscillationAmplitude();
        float oscFrequency = GetOscillationFrequency();

        pair.transform.position = new Vector3(WorldBounds.SpawnX, 0f, 0f);
        pair.Setup(gapCenterY, gapSize, speed, oscAmplitude, oscFrequency);

        _activeObstacles.Add(pair);
    }

    private float GetCurrentGapSize()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentGapSize;
        return _difficultyConfig.initialGapSize;
    }

    private float GetCurrentScrollSpeed()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentScrollSpeed;
        return _difficultyConfig.initialScrollSpeed;
    }

    private float GetCurrentSpawnInterval()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentSpawnInterval;
        return _difficultyConfig.spawnInterval;
    }

    private float GetCurrentGapYPadding()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentGapYPadding;
        return _difficultyConfig.initialGapYPadding;
    }

    private float GetCurrentSpeedVariance()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentSpeedVariance;
        return 0f;
    }

    private float GetCurrentOscillationAmplitude()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.CurrentOscillationAmplitude;
        return 0f;
    }

    private float GetOscillationFrequency()
    {
        if (DifficultyManager.Instance != null)
            return DifficultyManager.Instance.OscillationFrequency;
        return _difficultyConfig.oscillationFrequency;
    }

    private void ReturnOffScreenObstacles()
    {
        for (int i = _activeObstacles.Count - 1; i >= 0; i--)
        {
            if (_activeObstacles[i].transform.position.x < WorldBounds.DespawnX)
            {
                _pool.Return(_activeObstacles[i]);
                _activeObstacles.RemoveAt(i);
            }
        }
    }

    private void ClearAllObstacles()
    {
        for (int i = _activeObstacles.Count - 1; i >= 0; i--)
        {
            _pool.Return(_activeObstacles[i]);
        }
        _activeObstacles.Clear();
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                _spawning = true;
                _spawnTimer = _gracePeriod;
                break;
            case GameState.GameOver:
                _spawning = false;
                break;
            case GameState.Menu:
                _spawning = false;
                ClearAllObstacles();
                break;
        }
    }
}
