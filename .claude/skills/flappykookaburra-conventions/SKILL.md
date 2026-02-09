# FlappyKookaburra Conventions

## Singleton Pattern
All managers (GameManager, ScoreManager, DifficultyManager, AudioManager) use:
```csharp
public static T Instance { get; private set; }
private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

## Event Communication
Systems communicate via `static event Action<T>` delegates:
- Subscribe in `OnEnable()`, unsubscribe in `OnDisable()`
- GameManager.OnGameStateChanged is the primary state event
- ScoreManager.OnScoreChanged drives difficulty and UI updates
- Never poll state in Update — always use events

## ScriptableObject-Driven Data
All gameplay tuning lives in ScriptableObjects:
- `BirdStats` — flapForce, gravityScale, maxUpwardVelocity, rotationSpeed
- `DifficultyConfig` — gap size curves, speed curves, spawn interval curves
- `AudioConfig` — clip references, volume levels
- Reference via `[SerializeField]`, never hardcode constants

## Physics
- `Rigidbody2D` for player gravity and movement
- Velocity set (not AddForce) for consistent flap: `_rb.velocity = Vector2.up * force`
- `Collider2D` for solid collision, `BoxCollider2D` with `isTrigger` for score zones
- Use `CompareTag()` not string comparison

## Performance Rules
- Zero GC allocations in Update (no string concat, LINQ, boxing)
- Cache all GetComponent calls in Awake
- Use `Animator.StringToHash()` for animator parameters
- Object pool for obstacles and particles (never Instantiate during gameplay)

## Naming
- Classes/Files: `PascalCase.cs`
- Private fields: `_camelCase` with underscore prefix
- Inspector fields: `[SerializeField] private Type _fieldName`
- Tags: PascalCase (Player, Ground, Obstacle, ScoreZone)
- Layers: PascalCase (Player, Obstacle, ScoreZone)
- Sorting Layers: Background, Ground, Obstacles, Player
