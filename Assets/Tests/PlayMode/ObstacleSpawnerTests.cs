using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ObstacleSpawnerTests
{
    private GameObject _gameManagerGO;
    private GameManager _gameManager;
    private GameObject _spawnerGO;
    private ObstacleSpawner _spawner;
    private DifficultyConfig _difficultyConfig;
    private GameObject _obstaclePairPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager singleton
        _gameManagerGO = new GameObject("GameManager");
        _gameManager = _gameManagerGO.AddComponent<GameManager>();

        // Create a DifficultyConfig for testing
        _difficultyConfig = ScriptableObject.CreateInstance<DifficultyConfig>();
        _difficultyConfig.initialGapSize = 4f;
        _difficultyConfig.minimumGapSize = 2.5f;
        _difficultyConfig.initialScrollSpeed = 3f;
        _difficultyConfig.maximumScrollSpeed = 6f;
        _difficultyConfig.spawnInterval = 1.5f;
        _difficultyConfig.minimumSpawnInterval = 0.8f;

        // Create an ObstaclePair prefab with required child structure
        _obstaclePairPrefab = CreateObstaclePairPrefab();
        _obstaclePairPrefab.SetActive(false); // Prefabs are inactive

        // Create the spawner
        _spawnerGO = new GameObject("ObstacleSpawner");
        _spawner = _spawnerGO.AddComponent<ObstacleSpawner>();

        // Inject serialized fields via reflection
        var configField = typeof(ObstacleSpawner).GetField("_difficultyConfig",
            BindingFlags.NonPublic | BindingFlags.Instance);
        configField.SetValue(_spawner, _difficultyConfig);

        var prefabField = typeof(ObstacleSpawner).GetField("_obstaclePairPrefab",
            BindingFlags.NonPublic | BindingFlags.Instance);
        prefabField.SetValue(_spawner, _obstaclePairPrefab.GetComponent<ObstaclePair>());

        var poolSizeField = typeof(ObstacleSpawner).GetField("_poolSize",
            BindingFlags.NonPublic | BindingFlags.Instance);
        poolSizeField.SetValue(_spawner, 3);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_spawnerGO);
        Object.Destroy(_gameManagerGO);
        Object.Destroy(_difficultyConfig);
        Object.Destroy(_obstaclePairPrefab);

        // Clear singleton
        var instanceProp = typeof(GameManager).GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
        instanceProp.SetValue(null, null);
    }

    private GameObject CreateObstaclePairPrefab()
    {
        // Build an ObstaclePair with the child structure the scripts expect
        var pairGO = new GameObject("ObstaclePairPrefab");

        var topPipe = new GameObject("TopPipe");
        topPipe.transform.SetParent(pairGO.transform);
        topPipe.AddComponent<BoxCollider2D>();

        var bottomPipe = new GameObject("BottomPipe");
        bottomPipe.transform.SetParent(pairGO.transform);
        bottomPipe.AddComponent<BoxCollider2D>();

        var scoreZoneGO = new GameObject("ScoreZone");
        scoreZoneGO.transform.SetParent(pairGO.transform);
        var scoreZone = scoreZoneGO.AddComponent<ScoreZone>();

        // Add the Obstacle component (handles scrolling)
        pairGO.AddComponent<Obstacle>();

        // Add ObstaclePair and inject references
        var pair = pairGO.AddComponent<ObstaclePair>();

        var topField = typeof(ObstaclePair).GetField("_topPipe",
            BindingFlags.NonPublic | BindingFlags.Instance);
        topField.SetValue(pair, topPipe.transform);

        var bottomField = typeof(ObstaclePair).GetField("_bottomPipe",
            BindingFlags.NonPublic | BindingFlags.Instance);
        bottomField.SetValue(pair, bottomPipe.transform);

        var scoreZoneField = typeof(ObstaclePair).GetField("_scoreZone",
            BindingFlags.NonPublic | BindingFlags.Instance);
        scoreZoneField.SetValue(pair, scoreZone);

        return pairGO;
    }

    [UnityTest]
    public IEnumerator Spawner_DoesNotSpawn_InMenuState()
    {
        yield return null; // Allow Awake/Start

        // Spawner should not be spawning in Menu state
        var activeField = typeof(ObstacleSpawner).GetField("_activeObstacles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var activeObstacles = (List<ObstaclePair>)activeField.GetValue(_spawner);

        yield return new WaitForSeconds(2f);

        Assert.AreEqual(0, activeObstacles.Count,
            "Spawner should not spawn obstacles in Menu state");
    }

    [UnityTest]
    public IEnumerator Spawner_StartsSpawning_WhenGamePlaying()
    {
        yield return null;

        _gameManager.StartGame();

        // Wait longer than the spawn interval so at least one obstacle appears
        yield return new WaitForSeconds(_difficultyConfig.spawnInterval + 0.5f);

        var activeField = typeof(ObstacleSpawner).GetField("_activeObstacles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var activeObstacles = (List<ObstaclePair>)activeField.GetValue(_spawner);

        Assert.Greater(activeObstacles.Count, 0,
            "Spawner should have spawned at least one obstacle when game is Playing");
    }

    [UnityTest]
    public IEnumerator Spawner_StopsSpawning_OnGameOver()
    {
        yield return null;
        _gameManager.StartGame();

        // Wait for some obstacles to spawn
        yield return new WaitForSeconds(_difficultyConfig.spawnInterval + 0.5f);

        _gameManager.GameOver();
        yield return null;

        var spawningField = typeof(ObstacleSpawner).GetField("_spawning",
            BindingFlags.NonPublic | BindingFlags.Instance);
        bool isSpawning = (bool)spawningField.GetValue(_spawner);

        Assert.IsFalse(isSpawning,
            "Spawner should stop spawning on GameOver");
    }

    [UnityTest]
    public IEnumerator Spawner_ClearsObstacles_OnMenuState()
    {
        yield return null;
        _gameManager.StartGame();

        // Wait for obstacles to spawn
        yield return new WaitForSeconds(_difficultyConfig.spawnInterval + 0.5f);

        var activeField = typeof(ObstacleSpawner).GetField("_activeObstacles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var activeObstacles = (List<ObstaclePair>)activeField.GetValue(_spawner);

        Assert.Greater(activeObstacles.Count, 0,
            "Should have active obstacles before clearing");

        // Simulate returning to menu — invoke HandleGameStateChanged via the event
        _gameManager.GameOver();
        yield return null;

        // Use reflection to call SetState(Menu) on GameManager to trigger the event
        var setStateMethod = typeof(GameManager).GetMethod("SetState",
            BindingFlags.NonPublic | BindingFlags.Instance);
        setStateMethod.Invoke(_gameManager, new object[] { GameState.Menu });
        yield return null;

        Assert.AreEqual(0, activeObstacles.Count,
            "All active obstacles should be cleared when returning to Menu state");
    }

    [UnityTest]
    public IEnumerator SpawnTimer_ResetsOnGameStart()
    {
        yield return null;

        _gameManager.StartGame();
        yield return null;

        var timerField = typeof(ObstacleSpawner).GetField("_spawnTimer",
            BindingFlags.NonPublic | BindingFlags.Instance);
        float timer = (float)timerField.GetValue(_spawner);

        // Timer should be set to the spawn interval at game start
        Assert.AreEqual(_difficultyConfig.spawnInterval, timer, 0.1f,
            "Spawn timer should be initialized to spawn interval when game starts");
    }

    [UnityTest]
    public IEnumerator SpawnedObstacles_AreActive()
    {
        yield return null;
        _gameManager.StartGame();

        // Wait for at least one obstacle to spawn
        yield return new WaitForSeconds(_difficultyConfig.spawnInterval + 0.5f);

        var activeField = typeof(ObstacleSpawner).GetField("_activeObstacles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var activeObstacles = (List<ObstaclePair>)activeField.GetValue(_spawner);

        foreach (var obstacle in activeObstacles)
        {
            Assert.IsTrue(obstacle.gameObject.activeSelf,
                "Spawned obstacles should have their GameObjects active");
        }
    }

    [UnityTest]
    public IEnumerator MultipleSpawns_OverTime()
    {
        yield return null;
        _gameManager.StartGame();

        // Wait for multiple spawn intervals
        yield return new WaitForSeconds(_difficultyConfig.spawnInterval * 3f + 0.5f);

        var activeField = typeof(ObstacleSpawner).GetField("_activeObstacles",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var activeObstacles = (List<ObstaclePair>)activeField.GetValue(_spawner);

        Assert.GreaterOrEqual(activeObstacles.Count, 2,
            "Multiple obstacles should spawn over several spawn intervals");
    }
}
