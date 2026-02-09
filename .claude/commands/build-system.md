# Build System

Scaffold a new Unity game system following FlappyKookaburra's singleton manager pattern.

## Input

System name: $ARGUMENTS (e.g., ScoreManager, AudioManager, DifficultyManager)

## Process

### 1. Parse Arguments
Extract the system name (e.g., "ScoreManager" → class ScoreManager).

### 2. Check Existing Systems
Read `Assets/Scripts/Systems/` to see what patterns are in use.
Read `Assets/Scripts/Data/` for related ScriptableObject types.

### 3. Scaffold the System
Create `Assets/Scripts/Systems/{SystemName}.cs` with:

```csharp
using System;
using UnityEngine;

public class {SystemName} : MonoBehaviour
{
    public static {SystemName} Instance { get; private set; }

    // [SerializeField] fields for SO references and config

    // Public events
    // public static event Action<T> On{EventName};

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Handle state transitions
    }
}
```

### 4. Create ScriptableObject (if needed)
If the system needs configuration, create `Assets/Scripts/Data/{SystemName}Config.cs`:
```csharp
[CreateAssetMenu(fileName = "{SystemName}Config", menuName = "FlappyKookaburra/{SystemName}Config")]
public class {SystemName}Config : ScriptableObject
{
    // Config fields
}
```

### 5. Wire in Scene
- Create empty GameObject named "{SystemName}" in MainScene
- Attach the script
- Assign SO references if applicable

## Output
- Created system script
- Created SO definition (if applicable)
- Instructions for wiring in scene
