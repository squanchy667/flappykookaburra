# FlappyKookaburra — Project Conventions

## What This Is
Flappy Bird-style 2D game featuring a kookaburra navigating through eucalyptus tree obstacles in the Australian outback. Built in Unity with C#.

## Stack
- **Engine**: Unity 2022.3 LTS (2D, built-in render pipeline)
- **Language**: C# 9.0+
- **Physics**: Rigidbody2D, Collider2D, BoxCollider2D (trigger)
- **Data**: ScriptableObjects (BirdStats, DifficultyConfig, AudioConfig)
- **UI**: Unity UI + TextMeshPro
- **Animation**: Unity Animator (state machine with triggers)
- **Audio**: Unity AudioSource (SFX + ambient)
- **Testing**: Unity Test Framework (NUnit, Play Mode + Edit Mode)
- **Build**: WebGL (primary), Windows/macOS (dev)

## Structure
```
flappykookaburra/
├── Assets/
│   ├── Scripts/
│   │   ├── Systems/       # GameManager, ScoreManager, DifficultyManager, AudioManager, ParallaxLayer, ParticleSpawner, FrameRateManager, SafeAreaHandler, DifficultyVisualFeedback
│   │   ├── Player/        # PlayerController
│   │   ├── Obstacles/     # ObstacleSpawner, ObstaclePair, Obstacle, ScoreZone
│   │   ├── UI/            # UIManager, ScorePunchEffect
│   │   ├── Data/          # BirdStats, DifficultyConfig, AudioConfig (SO definitions)
│   │   └── Utils/         # ObjectPool, WorldBounds, ScreenShake
│   ├── Data/              # ScriptableObject instances (.asset files)
│   │   ├── Bird/          # DefaultBirdStats.asset
│   │   ├── Difficulty/    # DefaultDifficultyConfig.asset
│   │   └── Audio/         # DefaultAudioConfig.asset
│   ├── Prefabs/           # Kookaburra, ObstaclePair, ScoreParticle, DeathParticle
│   ├── Scenes/            # MainScene.unity
│   ├── Sprites/           # Kookaburra/, Background/, Obstacles/, Ground/
│   ├── Audio/             # flap.wav, kookaburra-laugh.wav, collision.wav, ambient.wav
│   ├── Animations/        # KookaburraAnimator.controller, clips
│   ├── UI/                # Fonts, UI sprites
│   └── Tests/
│       ├── PlayMode/      # PlayerControllerTests, ScoreManagerTests, ObstacleSpawnerTests, GameStateTests
│       └── EditMode/      # BirdStatsTests, DifficultyConfigTests, ObjectPoolTests
├── Packages/
└── ProjectSettings/
```

## Key Patterns

### Singletons
All managers use the singleton MonoBehaviour pattern:
```csharp
public static GameManager Instance { get; private set; }
private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

### Events
Systems communicate via C# events, not Unity messages:
```csharp
public static event Action<GameState> OnGameStateChanged;
// Subscribe in OnEnable, unsubscribe in OnDisable
```

### ScriptableObjects
All tunable values live in SOs — never hardcode gameplay constants:
```csharp
[SerializeField] BirdStats birdStats;
// Use birdStats.flapForce, not magic numbers
```

### Object Pooling
Use `ObjectPool<T>` from `Utils/` for anything spawned at runtime (obstacles, particles).

### Component References
Cache GetComponent calls in Awake, use `[RequireComponent]` for mandatory dependencies:
```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    private Rigidbody2D _rb;
    private void Awake() => _rb = GetComponent<Rigidbody2D>();
}
```

### Animator Parameters
Use hashed IDs, not strings:
```csharp
private static readonly int FlapHash = Animator.StringToHash("Flap");
_animator.SetTrigger(FlapHash);
```

## File Naming
- Scripts: `PascalCase.cs` (e.g., `PlayerController.cs`, `ObstacleSpawner.cs`)
- ScriptableObjects: `PascalCase.cs` for definition, `PascalCase.asset` for instances
- Prefabs: `PascalCase.prefab`
- Scenes: `PascalCase.unity`
- Sprites: `kebab-case.png`
- Audio: `kebab-case.wav`
- One class per file, filename matches class name

## Game State Flow
```
Menu → Playing → GameOver → Menu (restart)
```

## Sorting Layers (back to front)
Background → Ground → Obstacles → Player → UI

## Commands
```
Unity Editor: Ctrl+P (Play), Ctrl+Shift+B (Build Settings), Ctrl+S (Save)
Test Runner: Window → General → Test Runner → Run All
WebGL Build: File → Build Settings → WebGL → Build
Tools > Game > Setup Game: Full scene setup (managers, UI, prefabs, parallax, ground, visual feedback)
Tools > Game > Regenerate Prefabs: Delete and recreate all prefabs + re-wire scene
Tools > Build > iOS: Build for iOS (portrait locked, safe area)
Tools > Build > Android: Build for Android (portrait locked, safe area)
Tools > Build > Configure Mobile Settings: Apply portrait lock + hide status bar
```

## Commit Convention
- Format: `[Phase X] TXXX: Brief description`
- Branch: `feat/TXXX-task-name`

## Docs Repo
Paired docs at `../flappykookaburra-docs/` — see `TASK_BOARD.md` for task tracking.
