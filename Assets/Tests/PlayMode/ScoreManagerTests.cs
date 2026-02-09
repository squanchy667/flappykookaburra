using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ScoreManagerTests
{
    private GameObject _gameManagerGO;
    private GameManager _gameManager;
    private GameObject _scoreManagerGO;
    private ScoreManager _scoreManager;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager singleton first (ScoreManager depends on it)
        _gameManagerGO = new GameObject("GameManager");
        _gameManager = _gameManagerGO.AddComponent<GameManager>();

        // Create ScoreManager singleton
        _scoreManagerGO = new GameObject("ScoreManager");
        _scoreManager = _scoreManagerGO.AddComponent<ScoreManager>();

        // Clear any persisted high score for test isolation
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_scoreManagerGO);
        Object.Destroy(_gameManagerGO);

        // Clear singleton references
        var smInstance = typeof(ScoreManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        smInstance.SetValue(null, null);

        var gmInstance = typeof(GameManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        gmInstance.SetValue(null, null);

        // Clean up PlayerPrefs after tests
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
    }

    [UnityTest]
    public IEnumerator InitialScore_IsZero()
    {
        yield return null; // Allow Awake/Start to run

        Assert.AreEqual(0, _scoreManager.CurrentScore,
            "Initial score should be zero");
    }

    [UnityTest]
    public IEnumerator IncrementScore_IncreasesScoreByOne()
    {
        yield return null;

        _scoreManager.IncrementScore();

        Assert.AreEqual(1, _scoreManager.CurrentScore,
            "Score should be 1 after one increment");
    }

    [UnityTest]
    public IEnumerator IncrementScore_MultipleTimes_AccumulatesCorrectly()
    {
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();

        Assert.AreEqual(3, _scoreManager.CurrentScore,
            "Score should be 3 after three increments");
    }

    [UnityTest]
    public IEnumerator ResetScore_SetsScoreToZero()
    {
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.ResetScore();

        Assert.AreEqual(0, _scoreManager.CurrentScore,
            "Score should be zero after reset");
    }

    [UnityTest]
    public IEnumerator ScoreResets_WhenGameStateChangesToPlaying()
    {
        yield return null;

        // Start game, accumulate score
        _gameManager.StartGame();
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        Assert.AreEqual(2, _scoreManager.CurrentScore);

        // Trigger game over, then restart
        _gameManager.GameOver();
        yield return null;

        // Simulate restart by going back to Playing
        _gameManager.StartGame();
        yield return null;

        Assert.AreEqual(0, _scoreManager.CurrentScore,
            "Score should reset to zero when game restarts (enters Playing state)");
    }

    [UnityTest]
    public IEnumerator HighScore_UpdatesWhenCurrentScoreExceeds()
    {
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();

        Assert.AreEqual(3, _scoreManager.HighScore,
            "High score should update when current score exceeds it");
    }

    [UnityTest]
    public IEnumerator HighScore_DoesNotDecrease_OnReset()
    {
        yield return null;

        // Build up a high score
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        Assert.AreEqual(3, _scoreManager.HighScore);

        // Reset score (simulating new game)
        _scoreManager.ResetScore();

        Assert.AreEqual(3, _scoreManager.HighScore,
            "High score should not decrease when current score is reset");
    }

    [UnityTest]
    public IEnumerator HighScore_PersistsToPlayerPrefs()
    {
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();

        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        Assert.AreEqual(3, savedHighScore,
            "High score should be persisted to PlayerPrefs");
    }

    [UnityTest]
    public IEnumerator OnScoreChanged_EventFires_OnIncrement()
    {
        yield return null;

        int receivedScore = -1;
        ScoreManager.OnScoreChanged += (score) => receivedScore = score;

        _scoreManager.IncrementScore();

        Assert.AreEqual(1, receivedScore,
            "OnScoreChanged event should fire with new score on increment");

        // Clean up event subscription
        ScoreManager.OnScoreChanged -= (score) => receivedScore = score;
    }

    [UnityTest]
    public IEnumerator OnScoreChanged_EventFires_OnReset()
    {
        yield return null;

        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();

        int receivedScore = -1;
        ScoreManager.OnScoreChanged += (score) => receivedScore = score;

        _scoreManager.ResetScore();

        Assert.AreEqual(0, receivedScore,
            "OnScoreChanged event should fire with 0 on reset");
    }

    [UnityTest]
    public IEnumerator OnHighScoreChanged_EventFires_WhenNewHighScore()
    {
        yield return null;

        int receivedHighScore = -1;
        ScoreManager.OnHighScoreChanged += (score) => receivedHighScore = score;

        _scoreManager.IncrementScore();

        Assert.AreEqual(1, receivedHighScore,
            "OnHighScoreChanged event should fire when a new high score is achieved");
    }

    [UnityTest]
    public IEnumerator OnHighScoreChanged_DoesNotFire_WhenBelowHighScore()
    {
        yield return null;

        // Set an initial high score
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        _scoreManager.IncrementScore();
        Assert.AreEqual(3, _scoreManager.HighScore);

        // Reset and score below the high score
        _scoreManager.ResetScore();

        int receivedHighScore = -1;
        ScoreManager.OnHighScoreChanged += (score) => receivedHighScore = score;

        _scoreManager.IncrementScore(); // score=1, below high of 3

        Assert.AreEqual(-1, receivedHighScore,
            "OnHighScoreChanged should not fire when score is below the existing high score");
    }

    [UnityTest]
    public IEnumerator Singleton_OnlyOneInstanceExists()
    {
        yield return null;

        // Try to create a second ScoreManager
        var duplicateGO = new GameObject("ScoreManager2");
        duplicateGO.AddComponent<ScoreManager>();

        yield return null;

        // The duplicate should have been destroyed
        Assert.AreEqual(_scoreManager, ScoreManager.Instance,
            "Only one ScoreManager instance should exist (singleton pattern)");

        // Clean up if it wasn't destroyed yet (Destroy is deferred)
        Object.Destroy(duplicateGO);
    }
}
