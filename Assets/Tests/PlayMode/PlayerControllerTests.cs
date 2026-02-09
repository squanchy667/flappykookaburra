using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerControllerTests
{
    private GameObject _playerGO;
    private PlayerController _player;
    private Rigidbody2D _rb;
    private BirdStats _birdStats;
    private GameObject _gameManagerGO;
    private GameManager _gameManager;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager singleton (required by PlayerController)
        _gameManagerGO = new GameObject("GameManager");
        _gameManager = _gameManagerGO.AddComponent<GameManager>();

        // Create BirdStats ScriptableObject with test values
        _birdStats = ScriptableObject.CreateInstance<BirdStats>();
        _birdStats.flapForce = 5f;
        _birdStats.gravityScale = 2.5f;
        _birdStats.maxUpwardVelocity = 8f;
        _birdStats.rotationSpeed = 10f;
        _birdStats.deathRotation = -90f;

        // Create player GameObject with required components
        _playerGO = new GameObject("Player");
        _rb = _playerGO.AddComponent<Rigidbody2D>();
        _rb.gravityScale = 0f; // Start with no gravity for controlled tests
        _player = _playerGO.AddComponent<PlayerController>();

        // Inject BirdStats via serialized field using reflection
        var birdStatsField = typeof(PlayerController).GetField("_birdStats",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        birdStatsField.SetValue(_player, _birdStats);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_playerGO);
        Object.Destroy(_gameManagerGO);
        Object.Destroy(_birdStats);

        // Clear singleton reference so next test gets a fresh instance
        var instanceProp = typeof(GameManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        instanceProp.SetValue(null, null);
    }

    [UnityTest]
    public IEnumerator Flap_AppliesUpwardVelocity()
    {
        // Arrange: start the game so the player is in Playing state
        yield return null; // Allow Start() and Awake() to run
        _gameManager.StartGame();
        yield return null; // Allow state change to propagate

        // Act
        _player.Flap();
        yield return null;

        // Assert: velocity should be upward with flapForce magnitude
        Assert.AreEqual(_birdStats.flapForce, _rb.velocity.y, 0.01f,
            "Flap should set upward velocity equal to flapForce");
    }

    [UnityTest]
    public IEnumerator Flap_ClampsToMaxUpwardVelocity()
    {
        // Arrange: use a very high flap force that exceeds max
        _birdStats.flapForce = 20f;
        _birdStats.maxUpwardVelocity = 8f;

        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Act
        _player.Flap();
        yield return null;

        // Assert: velocity should be clamped to maxUpwardVelocity
        Assert.LessOrEqual(_rb.velocity.y, _birdStats.maxUpwardVelocity,
            "Flap velocity should be clamped to maxUpwardVelocity");
    }

    [UnityTest]
    public IEnumerator Flap_DoesNothing_WhenDead()
    {
        // Arrange: start game, then trigger game over via collision
        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Simulate death by triggering GameOver
        _gameManager.GameOver();
        yield return null;

        float velocityBeforeFlap = _rb.velocity.y;

        // Act: try to flap after death
        _player.Flap();
        yield return null;

        // Assert: velocity should not change after flap when dead
        Assert.AreEqual(velocityBeforeFlap, _rb.velocity.y, 0.01f,
            "Flap should have no effect when the player is dead");
    }

    [UnityTest]
    public IEnumerator PlayerPhysics_NotSimulated_InMenuState()
    {
        // Arrange & Act: wait for Start() which sets rb.simulated = false
        yield return null;

        // Assert: Rigidbody should not be simulated in Menu state
        Assert.IsFalse(_rb.simulated,
            "Rigidbody should not be simulated during Menu state");
    }

    [UnityTest]
    public IEnumerator PlayerPhysics_Simulated_InPlayingState()
    {
        // Arrange
        yield return null;

        // Act
        _gameManager.StartGame();
        yield return null;

        // Assert: Rigidbody should be simulated when Playing
        Assert.IsTrue(_rb.simulated,
            "Rigidbody should be simulated during Playing state");
    }

    [UnityTest]
    public IEnumerator GravityScale_SetFromBirdStats_WhenPlaying()
    {
        // Arrange
        _birdStats.gravityScale = 3.5f;
        yield return null;

        // Act
        _gameManager.StartGame();
        yield return null;

        // Assert
        Assert.AreEqual(_birdStats.gravityScale, _rb.gravityScale, 0.01f,
            "Gravity scale should match BirdStats when entering Playing state");
    }

    [UnityTest]
    public IEnumerator Velocity_ResetToZero_WhenGameStarts()
    {
        // Arrange: give the rigidbody some velocity
        yield return null;
        _rb.simulated = true;
        _rb.velocity = new Vector2(1f, -5f);

        // Act
        _gameManager.StartGame();
        yield return null;

        // Assert
        Assert.AreEqual(Vector2.zero, _rb.velocity,
            "Velocity should be reset to zero when game starts");
    }

    [UnityTest]
    public IEnumerator Gravity_PullsPlayerDown_WhenPlaying()
    {
        // Arrange
        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Record starting position
        float startY = _playerGO.transform.position.y;

        // Act: wait several frames for gravity to take effect
        yield return new WaitForSeconds(0.3f);

        // Assert: player should have fallen
        float endY = _playerGO.transform.position.y;
        Assert.Less(endY, startY,
            "Player should fall due to gravity when in Playing state");
    }

    [UnityTest]
    public IEnumerator Collision_TriggersDeathAndGameOver()
    {
        // Arrange: set up player with collider for collision detection
        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Create an obstacle to collide with
        var obstacleGO = new GameObject("Obstacle");
        var obstacleRb = obstacleGO.AddComponent<Rigidbody2D>();
        obstacleRb.bodyType = RigidbodyType2D.Static;
        var obstacleCollider = obstacleGO.AddComponent<BoxCollider2D>();

        // Add collider to player
        var playerCollider = _playerGO.AddComponent<BoxCollider2D>();

        // Move obstacle to player position to cause collision
        obstacleGO.transform.position = _playerGO.transform.position;

        // Wait for physics to detect the collision
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return null;

        // Assert: game should be in GameOver state after collision
        Assert.AreEqual(GameState.GameOver, _gameManager.CurrentState,
            "Game should transition to GameOver after player collision");

        // Cleanup
        Object.Destroy(obstacleGO);
    }

    [UnityTest]
    public IEnumerator DeathRotation_AppliedOnDeath()
    {
        // Arrange
        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Create obstacle for collision
        var obstacleGO = new GameObject("Obstacle");
        var obstacleRb = obstacleGO.AddComponent<Rigidbody2D>();
        obstacleRb.bodyType = RigidbodyType2D.Static;
        obstacleGO.AddComponent<BoxCollider2D>();
        _playerGO.AddComponent<BoxCollider2D>();

        obstacleGO.transform.position = _playerGO.transform.position;

        // Wait for collision
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return null;

        // Assert: rotation should be set to deathRotation on z-axis
        float zRotation = _playerGO.transform.eulerAngles.z;
        // Normalize angle to -180..180 range
        if (zRotation > 180f) zRotation -= 360f;
        Assert.AreEqual(_birdStats.deathRotation, zRotation, 1f,
            "Player rotation should be set to deathRotation after dying");

        Object.Destroy(obstacleGO);
    }

    [UnityTest]
    public IEnumerator MultipleFlaps_EachResetsVelocity()
    {
        // Arrange
        yield return null;
        _gameManager.StartGame();
        yield return null;

        // Act: flap, wait for gravity to pull down, then flap again
        _player.Flap();
        yield return new WaitForSeconds(0.2f);

        float velocityBeforeSecondFlap = _rb.velocity.y;
        _player.Flap();
        yield return null;

        // Assert: second flap should reset velocity to flapForce
        Assert.AreEqual(_birdStats.flapForce, _rb.velocity.y, 0.01f,
            "Each flap should reset velocity to flapForce regardless of current velocity");
    }
}
