# FlappyKookaburra Workspace

## Project Layout
```
FlappyKookaburra/
├── flappykookaburra/              ← Code repo (Unity project)
│   ├── .claude/                   ← Claude config (agents, commands, skills)
│   ├── Assets/
│   │   ├── Scripts/               ← All C# code
│   │   │   ├── Systems/           ← Managers (GameManager, ScoreManager, etc.)
│   │   │   ├── Player/            ← PlayerController
│   │   │   ├── Obstacles/         ← ObstacleSpawner, ObstaclePair, ScoreZone
│   │   │   ├── UI/                ← UIManager, effects
│   │   │   ├── Data/              ← ScriptableObject definitions (.cs)
│   │   │   └── Utils/             ← ObjectPool, WorldBounds, ScreenShake
│   │   ├── Data/                  ← ScriptableObject instances (.asset)
│   │   ├── Prefabs/               ← Kookaburra, ObstaclePair, particles
│   │   ├── Scenes/                ← MainScene.unity
│   │   ├── Sprites/               ← All 2D art
│   │   ├── Audio/                 ← Sound effects and music
│   │   ├── Animations/            ← Animator controllers and clips
│   │   ├── UI/                    ← Fonts, UI sprites
│   │   └── Tests/                 ← PlayMode/ and EditMode/
│   ├── Packages/
│   └── ProjectSettings/
│
└── flappykookaburra-docs/         ← Docs repo (GitBook)
    ├── README.md, PLAN.md, SUMMARY.md, TASK_BOARD.md
    ├── architecture/              ← System overview, data flow
    ├── developer/                 ← Setup guide, coding standards, game systems
    ├── product/                   ← Features, roadmap
    ├── resources/                 ← Tech stack, changelog, known issues
    ├── testing/                   ← Test plan
    └── tasks/phase-{1-4}/         ← Task specs (T001–T020)
```

## Key Files
| File | Purpose |
|------|---------|
| `Assets/Scripts/Systems/GameManager.cs` | Central state machine |
| `Assets/Scripts/Systems/GameState.cs` | Menu, Playing, GameOver enum |
| `Assets/Scripts/Player/PlayerController.cs` | Kookaburra flap mechanics |
| `Assets/Scripts/Obstacles/ObstacleSpawner.cs` | Procedural obstacle generation |
| `Assets/Scripts/Data/BirdStats.cs` | Player tuning SO |
| `Assets/Scripts/Data/DifficultyConfig.cs` | Difficulty curves SO |
| `Assets/Scenes/MainScene.unity` | The one and only scene |
| `../flappykookaburra-docs/TASK_BOARD.md` | Task tracking |

## Adding New Code
- New system → `Assets/Scripts/Systems/{Name}.cs` + singleton pattern
- New SO type → `Assets/Scripts/Data/{Name}.cs` + instance in `Assets/Data/`
- New prefab → create in `Assets/Prefabs/`
- New test → `Assets/Tests/{PlayMode,EditMode}/{Name}Tests.cs`
