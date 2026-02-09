using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameStateTests
{
    private GameObject _gameManagerGO;
    private GameManager _gameManager;

    [SetUp]
    public void SetUp()
    {
        _gameManagerGO = new GameObject("GameManager");
        _gameManager = _gameManagerGO.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_gameManagerGO);

        // Clear singleton reference
        var instanceProp = typeof(GameManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        instanceProp.SetValue(null, null);
    }

    [UnityTest]
    public IEnumerator InitialState_IsMenu()
    {
        // Wait for Start() to run, which calls SetState(Menu)
        yield return null;

        Assert.AreEqual(GameState.Menu, _gameManager.CurrentState,
            "Initial game state should be Menu");
    }

    [UnityTest]
    public IEnumerator StartGame_TransitionsToPlaying()
    {
        yield return null;

        _gameManager.StartGame();

        Assert.AreEqual(GameState.Playing, _gameManager.CurrentState,
            "State should be Playing after StartGame()");
    }

    [UnityTest]
    public IEnumerator GameOver_TransitionsToGameOver()
    {
        yield return null;

        _gameManager.StartGame();
        _gameManager.GameOver();

        Assert.AreEqual(GameState.GameOver, _gameManager.CurrentState,
            "State should be GameOver after GameOver()");
    }

    [UnityTest]
    public IEnumerator FullGameCycle_MenuToPlayingToGameOver()
    {
        yield return null;

        // Verify initial state
        Assert.AreEqual(GameState.Menu, _gameManager.CurrentState);

        // Transition to Playing
        _gameManager.StartGame();
        Assert.AreEqual(GameState.Playing, _gameManager.CurrentState);

        // Transition to GameOver
        _gameManager.GameOver();
        Assert.AreEqual(GameState.GameOver, _gameManager.CurrentState);
    }

    [UnityTest]
    public IEnumerator OnGameStateChanged_EventFires_WithCorrectState()
    {
        yield return null;

        GameState receivedState = GameState.Menu;
        int eventCount = 0;
        GameManager.OnGameStateChanged += (state) =>
        {
            receivedState = state;
            eventCount++;
        };

        _gameManager.StartGame();

        Assert.AreEqual(GameState.Playing, receivedState,
            "Event should fire with Playing state");
        Assert.AreEqual(1, eventCount,
            "Event should have fired exactly once for StartGame");
    }

    [UnityTest]
    public IEnumerator OnGameStateChanged_FiresForEachTransition()
    {
        yield return null;

        var receivedStates = new System.Collections.Generic.List<GameState>();
        GameManager.OnGameStateChanged += (state) => receivedStates.Add(state);

        _gameManager.StartGame();
        _gameManager.GameOver();

        Assert.AreEqual(2, receivedStates.Count,
            "Event should fire for each state transition");
        Assert.AreEqual(GameState.Playing, receivedStates[0],
            "First transition should be to Playing");
        Assert.AreEqual(GameState.GameOver, receivedStates[1],
            "Second transition should be to GameOver");
    }

    [UnityTest]
    public IEnumerator Singleton_InstanceIsSet()
    {
        yield return null;

        Assert.IsNotNull(GameManager.Instance,
            "GameManager.Instance should be set after Awake");
        Assert.AreEqual(_gameManager, GameManager.Instance,
            "GameManager.Instance should reference our created instance");
    }

    [UnityTest]
    public IEnumerator Singleton_DuplicateIsDestroyed()
    {
        yield return null;

        // Create a duplicate GameManager
        var duplicateGO = new GameObject("GameManager2");
        duplicateGO.AddComponent<GameManager>();

        yield return null; // Allow Destroy to process

        // Instance should still be the original
        Assert.AreEqual(_gameManager, GameManager.Instance,
            "Singleton Instance should remain the original GameManager");

        Object.Destroy(duplicateGO);
    }

    [UnityTest]
    public IEnumerator MultipleListeners_AllReceiveStateChange()
    {
        yield return null;

        bool listener1Fired = false;
        bool listener2Fired = false;
        bool listener3Fired = false;

        GameManager.OnGameStateChanged += (state) => listener1Fired = true;
        GameManager.OnGameStateChanged += (state) => listener2Fired = true;
        GameManager.OnGameStateChanged += (state) => listener3Fired = true;

        _gameManager.StartGame();

        Assert.IsTrue(listener1Fired, "Listener 1 should have been notified");
        Assert.IsTrue(listener2Fired, "Listener 2 should have been notified");
        Assert.IsTrue(listener3Fired, "Listener 3 should have been notified");
    }

    [UnityTest]
    public IEnumerator StateProperty_ReflectsCurrentState()
    {
        yield return null;

        Assert.AreEqual(GameState.Menu, _gameManager.CurrentState);

        _gameManager.StartGame();
        Assert.AreEqual(GameState.Playing, _gameManager.CurrentState);

        _gameManager.GameOver();
        Assert.AreEqual(GameState.GameOver, _gameManager.CurrentState);
    }

    [UnityTest]
    public IEnumerator GameState_Enum_HasExpectedValues()
    {
        yield return null;

        // Verify the enum has all expected values
        Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), "Menu"),
            "GameState should have a Menu value");
        Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), "Playing"),
            "GameState should have a Playing value");
        Assert.IsTrue(System.Enum.IsDefined(typeof(GameState), "GameOver"),
            "GameState should have a GameOver value");

        // Verify total count
        var values = System.Enum.GetValues(typeof(GameState));
        Assert.AreEqual(3, values.Length,
            "GameState enum should have exactly 3 values");
    }

    [UnityTest]
    public IEnumerator EventSubscription_CanUnsubscribeCleanly()
    {
        yield return null;

        int callCount = 0;
        System.Action<GameState> handler = (state) => callCount++;

        GameManager.OnGameStateChanged += handler;
        _gameManager.StartGame();
        Assert.AreEqual(1, callCount, "Handler should be called once after subscribing");

        GameManager.OnGameStateChanged -= handler;
        _gameManager.GameOver();
        Assert.AreEqual(1, callCount,
            "Handler should not be called after unsubscribing");
    }

    [UnityTest]
    public IEnumerator IntegrationTest_ScoreManagerResetsOnPlaying()
    {
        // Integration test: verify ScoreManager listens to GameManager state changes
        var scoreManagerGO = new GameObject("ScoreManager");
        var scoreManager = scoreManagerGO.AddComponent<ScoreManager>();

        yield return null; // Allow Awake/OnEnable to subscribe

        _gameManager.StartGame();
        yield return null;

        scoreManager.IncrementScore();
        scoreManager.IncrementScore();
        Assert.AreEqual(2, scoreManager.CurrentScore);

        // Transition back to Playing (simulating restart)
        _gameManager.GameOver();
        yield return null;

        // Use reflection to trigger Playing state again
        var setStateMethod = typeof(GameManager).GetMethod("SetState",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        setStateMethod.Invoke(_gameManager, new object[] { GameState.Playing });
        yield return null;

        Assert.AreEqual(0, scoreManager.CurrentScore,
            "ScoreManager should reset score when game enters Playing state");

        // Cleanup
        var smInstance = typeof(ScoreManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        smInstance.SetValue(null, null);
        Object.Destroy(scoreManagerGO);
    }
}
