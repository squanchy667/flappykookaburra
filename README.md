# FlappyKookaburra

A Flappy Bird-style game featuring a kookaburra as the player character. Tap to flap through obstacles in the Australian outback!

## Stack

- **Engine:** Unity
- **Language:** C#
- **Data:** ScriptableObjects

## Getting Started

1. Open project in Unity Hub
2. Open `Assets/Scenes/MainScene.unity`
3. Press Play

## Structure

```
flappykookaburra/
├── Assets/
│   ├── Scripts/         # C# game scripts
│   │   ├── Systems/     # Game systems (GameManager, ScoreManager, etc.)
│   │   ├── Player/      # Kookaburra controller, animations
│   │   ├── Obstacles/   # Pipe/obstacle generation and movement
│   │   ├── UI/          # UI components (menus, HUD, game over)
│   │   └── Utils/       # Utilities
│   ├── Data/            # ScriptableObjects (difficulty, bird stats)
│   ├── Prefabs/         # Prefab assets
│   ├── Scenes/          # Unity scenes
│   ├── Sprites/         # 2D art (kookaburra, pipes, background)
│   ├── Audio/           # Sound effects (kookaburra laugh, flap, score)
│   └── UI/              # UI assets
├── Packages/            # Unity packages
└── ProjectSettings/     # Unity project settings
```
