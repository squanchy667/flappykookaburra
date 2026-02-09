# FlappyKookaburra — Project Conventions

## What This Is
Flappy Bird-style 2D game featuring a kookaburra navigating through eucalyptus tree obstacles in the Australian outback. Built in Unity with C#. Features a multi-part rigged bird, modular obstacles, DOTween-powered UI choreography, and a medal system.

## Stack
- **Engine**: Unity 2022.3 LTS (2D, built-in render pipeline)
- **Language**: C# 9.0+
- **Physics**: Rigidbody2D, Collider2D, BoxCollider2D (trigger)
- **Data**: ScriptableObjects (BirdStats, DifficultyConfig, AudioConfig, UIColorPalette, MedalConfig)
- **UI**: Unity UI + TextMeshPro
- **Animation**: Unity Animator (rotation-based child transform curves for wing rig)
- **Tweening**: DOTween (lightweight implementation in Assets/Plugins/DOTween/)
- **Audio**: Unity AudioSource (SFX + ambient)
- **Testing**: Unity Test Framework (NUnit, Play Mode + Edit Mode)
- **Build**: WebGL (primary), Windows/macOS (dev)

## Structure
```
flappykookaburra/
├── Assets/
│   ├── Scripts/
│   │   ├── Systems/       # GameManager, ScoreManager, DifficultyManager, AudioManager,
│   │   │                  # ParallaxLayer, ParticleSpawner, FrameRateManager, SafeAreaHandler,
│   │   │                  # DifficultyVisualFeedback, CloudSpawner, DOTweenInitializer
│   │   ├── Player/        # PlayerController, KookaburraRig
│   │   ├── Obstacles/     # ObstacleSpawner, ObstaclePair, Obstacle, ScoreZone
│   │   ├── UI/            # UIManager, ScorePunchEffect, GameOverSequence,
│   │   │                  # TitleScreenAnimator, DeathSequence, ScreenFlash,
│   │   │                  # ButtonAnimator, MedalDisplay, SpriteScoreDisplay
│   │   ├── Data/          # BirdStats, DifficultyConfig, AudioConfig,
│   │   │                  # UIColorPalette, MedalConfig (SO definitions)
│   │   └── Utils/         # ObjectPool, WorldBounds, ScreenShake
│   ├── Plugins/
│   │   └── DOTween/       # DOTween.cs, DOTweenExtensions.cs (lightweight tween library)
│   ├── Data/              # ScriptableObject instances (.asset files)
│   │   ├── Bird/          # DefaultBirdStats.asset
│   │   ├── Difficulty/    # DefaultDifficultyConfig.asset
│   │   ├── Audio/         # DefaultAudioConfig.asset
│   │   └── UI/            # DefaultUIColorPalette.asset, DefaultMedalConfig.asset
│   ├── Prefabs/           # Kookaburra (5-part rig), ObstaclePair (modular), ScoreParticle, DeathParticle
│   ├── Scenes/            # MainScene.unity
│   ├── Sprites/
│   │   ├── Kookaburra/    # kookaburra-body.png, kookaburra-wing.png, kookaburra-tail.png, kookaburra-eye.png
│   │   ├── Background/    # sky-gradient, hills-silhouette, trees-midground, cloud-small, cloud-large
│   │   ├── Obstacles/     # trunk-segment.png, trunk-cap-top.png, trunk-cap-bottom.png
│   │   ├── Ground/        # ground-surface.png, ground-sub.png
│   │   └── UI/            # digit-0..9.png, medal-*.png, button-bg.png, title-logo.png
│   ├── Audio/             # flap.wav, kookaburra-laugh.wav, collision.wav, ambient.wav
│   ├── Animations/        # KookaburraAnimator.controller, Idle/Flap/Glide/Die/Blink clips
│   ├── UI/
│   │   └── Fonts/         # FredokaOne-Regular.ttf (Google Fonts, OFL)
│   └── Tests/
│       ├── PlayMode/
│       └── EditMode/
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

### DOTween Patterns
- Use `SetUpdate(true)` on all UI tweens to work during `timeScale = 0` or slow-mo
- Always `DOTween.Kill(target)` before starting new tweens on same target
- Use `DOTween.KillAll()` before scene transitions to prevent leaks
- Prefer DOTween Sequences for staged reveals (GameOverSequence)
- Use `SetEase(Ease.OutBack)` for bouncy UI, `OutElastic` for medal pop
- DOTweenInitializer auto-inits via `[RuntimeInitializeOnLoadMethod]`

### Bird Rig
5-part hierarchy (body, 2 wings, tail, eye) under root with physics:
```
Kookaburra (root - Rigidbody2D, CircleCollider2D, PlayerController, KookaburraRig)
  +-- Body (SpriteRenderer, sortingOrder: 1)
  +-- LeftWing (SpriteRenderer, sortingOrder: 0, pivot at shoulder)
  +-- RightWing (SpriteRenderer, sortingOrder: 2, pivot at shoulder)
  +-- Tail (SpriteRenderer, sortingOrder: 0)
  +-- Eye (SpriteRenderer, sortingOrder: 3)
```
Animation uses rotation curves on child transforms (not root scale).

### Modular Obstacles
Each pipe = Trunk (SpriteRenderer drawMode:Tiled) + Cap at gap edge.

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
Menu (animated title) → Playing → Die (flash, shake, slow-mo) → GameOver (choreographed) → Menu (restart)
```

## Sorting Layers (back to front)
Background → Ground → Obstacles → Player → UI

## Commands
```
Unity Editor: Ctrl+P (Play), Ctrl+Shift+B (Build Settings), Ctrl+S (Save)
Test Runner: Window → General → Test Runner → Run All
WebGL Build: File → Build Settings → WebGL → Build
Tools > Game > Setup Game: Full scene setup (managers, UI, prefabs, parallax, clouds, visual feedback)
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
