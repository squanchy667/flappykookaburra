using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using TMPro;

public static class GameSetup
{
    [MenuItem("Tools/Game/Regenerate Prefabs")]
    public static void RegeneratePrefabs()
    {
        if (!EditorUtility.DisplayDialog("Regenerate Prefabs",
            "This will delete and recreate all prefabs, animations, and re-wire the scene.\n\nContinue?",
            "Yes", "Cancel"))
            return;

        // Delete existing prefabs so they get recreated with correct settings
        string[] assetPaths = {
            "Assets/Prefabs/ObstaclePair.prefab",
            "Assets/Prefabs/Kookaburra.prefab",
            "Assets/Prefabs/ScoreParticle.prefab",
            "Assets/Prefabs/DeathParticle.prefab",
            "Assets/Animations/KookaburraAnimator.controller",
            "Assets/Animations/Idle.anim",
            "Assets/Animations/Flap.anim"
        };
        foreach (var path in assetPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[GameSetup] Deleted {path}");
            }
        }

        // Also destroy the scene Kookaburra instance so it gets re-instantiated from new prefab
        var scenePlayer = GameObject.Find("Kookaburra");
        if (scenePlayer != null)
        {
            Object.DestroyImmediate(scenePlayer);
            Debug.Log("[GameSetup] Removed old Kookaburra scene instance");
        }

        AssetDatabase.Refresh();

        // Run setup directly (no second dialog)
        SetupGameCore();
    }

    [MenuItem("Tools/Game/Setup Game")]
    public static void SetupGame()
    {
        if (!EditorUtility.DisplayDialog("Setup Game",
            "This will create ScriptableObject assets, prefabs, and set up the current scene with all managers and UI.\n\nContinue?",
            "Yes", "Cancel"))
            return;

        SetupGameCore();
    }

    private static void SetupGameCore()
    {
        CreateFolders();
        var birdStats = CreateBirdStats();
        var difficultyConfig = CreateDifficultyConfig();
        var audioConfig = CreateAudioConfig();
        var obstaclePairPrefab = CreateObstaclePairPrefab();
        var scoreParticlePrefab = CreateScoreParticlePrefab();
        var deathParticlePrefab = CreateDeathParticlePrefab();
        var kookaburaPrefab = CreateKookaburaPrefab(birdStats);
        SetupScene(birdStats, difficultyConfig, audioConfig, obstaclePairPrefab, scoreParticlePrefab, deathParticlePrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GameSetup] Setup complete! Review the scene and press Play.");
    }

    private static void CreateFolders()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Bird");
        EnsureFolder("Assets/Data/Difficulty");
        EnsureFolder("Assets/Data/Audio");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Sprites");
        EnsureFolder("Assets/Sprites/Kookaburra");
        EnsureFolder("Assets/Sprites/Background");
        EnsureFolder("Assets/Sprites/Obstacles");
        EnsureFolder("Assets/Audio");
        EnsureFolder("Assets/Animations");
        EnsureFolder("Assets/UI");
    }

    // ─── ScriptableObjects ────────────────────────────────────────

    private static BirdStats CreateBirdStats()
    {
        const string path = "Assets/Data/Bird/DefaultBirdStats.asset";
        var existing = AssetDatabase.LoadAssetAtPath<BirdStats>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<BirdStats>();
        so.flapForce = 5.5f;
        so.gravityScale = 2.5f;
        so.maxUpwardVelocity = 8f;
        so.rotationSpeed = 10f;
        so.deathRotation = -90f;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log("[GameSetup] Created BirdStats");
        return so;
    }

    private static DifficultyConfig CreateDifficultyConfig()
    {
        const string path = "Assets/Data/Difficulty/DefaultDifficultyConfig.asset";
        var existing = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<DifficultyConfig>();
        so.initialGapSize = 4f;
        so.minimumGapSize = 2.5f;
        so.gapSizeCurve = AnimationCurve.Linear(0f, 0f, 50f, 1f);
        so.initialScrollSpeed = 3f;
        so.maximumScrollSpeed = 6f;
        so.scrollSpeedCurve = AnimationCurve.Linear(0f, 0f, 50f, 1f);
        so.spawnInterval = 1.5f;
        so.minimumSpawnInterval = 0.8f;
        so.spawnIntervalCurve = AnimationCurve.Linear(0f, 0f, 50f, 1f);
        AssetDatabase.CreateAsset(so, path);
        Debug.Log("[GameSetup] Created DifficultyConfig");
        return so;
    }

    private static AudioConfig CreateAudioConfig()
    {
        const string path = "Assets/Data/Audio/DefaultAudioConfig.asset";
        var existing = AssetDatabase.LoadAssetAtPath<AudioConfig>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<AudioConfig>();
        so.sfxVolume = 1f;
        so.musicVolume = 0.5f;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log("[GameSetup] Created AudioConfig (assign audio clips later)");
        return so;
    }

    // ─── Sprites ─────────────────────────────────────────────────

    private static Sprite LoadOrCreateSprite(string name, Color fallbackColor, int fallbackWidth, int fallbackHeight, int pixelsPerUnit = 64)
    {
        string path = $"Assets/Sprites/{name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (existing == null)
        {
            // No image found — create a solid-color placeholder
            var tex = new Texture2D(fallbackWidth, fallbackHeight);
            var pixels = new Color[fallbackWidth * fallbackHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fallbackColor;
            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        // Always ensure correct import settings
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ─── Prefabs ──────────────────────────────────────────────────

    private static ObstaclePair CreateObstaclePairPrefab()
    {
        const string path = "Assets/Prefabs/ObstaclePair.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<ObstaclePair>(path);
        if (existing != null) return existing;

        var pipeSprite = LoadOrCreateSprite("Obstacles/pipe", new Color(0.2f, 0.7f, 0.2f), 32, 256, pixelsPerUnit: 64);

        // Calculate scale dynamically so pipe covers _pipeHeight (20 units) regardless of sprite size
        float spriteHeight = pipeSprite.bounds.size.y;
        float spriteWidth = pipeSprite.bounds.size.x;
        const float targetPipeHeight = 20f;
        const float targetPipeWidth = 1.5f;
        float scaleY = targetPipeHeight / spriteHeight;
        float scaleX = targetPipeWidth / spriteWidth;

        // Root
        var root = new GameObject("ObstaclePair");
        var obstacle = root.AddComponent<Obstacle>();
        var pair = root.AddComponent<ObstaclePair>();

        // Top pipe — scaled to match _pipeHeight
        var top = new GameObject("TopPipe");
        top.transform.SetParent(root.transform);
        top.transform.localPosition = new Vector3(0, 5f, 0);
        top.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        var topSr = top.AddComponent<SpriteRenderer>();
        topSr.sprite = pipeSprite;
        topSr.flipY = true;
        topSr.sortingLayerName = "Obstacles";
        var topCol = top.AddComponent<BoxCollider2D>();

        // Bottom pipe — same scale as top
        var bottom = new GameObject("BottomPipe");
        bottom.transform.SetParent(root.transform);
        bottom.transform.localPosition = new Vector3(0, -5f, 0);
        bottom.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        var bottomSr = bottom.AddComponent<SpriteRenderer>();
        bottomSr.sprite = pipeSprite;
        bottomSr.sortingLayerName = "Obstacles";
        var bottomCol = bottom.AddComponent<BoxCollider2D>();

        Debug.Log($"[GameSetup] Pipe sprite: {spriteWidth}x{spriteHeight} world units → scale ({scaleX:F2}, {scaleY:F2})");

        // Score zone (trigger between pipes)
        var zone = new GameObject("ScoreZone");
        zone.transform.SetParent(root.transform);
        zone.transform.localPosition = Vector3.zero;
        var zoneCol = zone.AddComponent<BoxCollider2D>();
        zoneCol.isTrigger = true;
        zoneCol.size = new Vector2(0.5f, 4f);
        var scoreZone = zone.AddComponent<ScoreZone>();

        // Wire serialized fields via SerializedObject
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        var so = new SerializedObject(prefab.GetComponent<ObstaclePair>());
        so.FindProperty("_topPipe").objectReferenceValue = prefab.transform.Find("TopPipe");
        so.FindProperty("_bottomPipe").objectReferenceValue = prefab.transform.Find("BottomPipe");
        so.FindProperty("_scoreZone").objectReferenceValue = prefab.transform.Find("ScoreZone").GetComponent<ScoreZone>();
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created ObstaclePair prefab");
        return prefab.GetComponent<ObstaclePair>();
    }

    private static ParticleSystem CreateScoreParticlePrefab()
    {
        const string path = "Assets/Prefabs/ScoreParticle.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
        if (existing != null) return existing;

        var go = new GameObject("ScoreParticle");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.4f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.85f, 0f); // gold
        main.maxParticles = 20;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "UI";

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[GameSetup] Created ScoreParticle prefab");
        return prefab.GetComponent<ParticleSystem>();
    }

    private static ParticleSystem CreateDeathParticlePrefab()
    {
        const string path = "Assets/Prefabs/DeathParticle.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
        if (existing != null) return existing;

        var go = new GameObject("DeathParticle");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration = 0.6f;
        main.startLifetime = 0.5f;
        main.startSpeed = 4f;
        main.startSize = 0.2f;
        main.startColor = Color.red;
        main.maxParticles = 30;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "UI";

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[GameSetup] Created DeathParticle prefab");
        return prefab.GetComponent<ParticleSystem>();
    }

    private static GameObject CreateKookaburaPrefab(BirdStats stats)
    {
        const string path = "Assets/Prefabs/Kookaburra.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var birdSprite = LoadOrCreateSprite("Kookaburra/kookaburra", new Color(0.6f, 0.4f, 0.2f), 32, 32, pixelsPerUnit: 960);

        var go = new GameObject("Kookaburra");
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = birdSprite;
        sr.sortingLayerName = "Player";

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = stats.gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = CreateKookaburraAnimator();

        var pc = go.AddComponent<PlayerController>();

        // Wire serialized fields
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        var so = new SerializedObject(prefab.GetComponent<PlayerController>());
        so.FindProperty("_birdStats").objectReferenceValue = stats;
        so.FindProperty("_animator").objectReferenceValue = prefab.GetComponent<Animator>();
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created Kookaburra prefab with wing animation");
        return prefab;
    }

    private static AnimatorController CreateKookaburraAnimator()
    {
        const string controllerPath = "Assets/Animations/KookaburraAnimator.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null) return existing;

        EnsureFolder("Assets/Animations");

        // ── Idle clip: continuous wing flap via scale oscillation ──
        var idleClip = new AnimationClip { name = "Idle" };
        // Wings down (squish Y, stretch X) → wings up (stretch Y, squish X)
        idleClip.SetCurve("", typeof(Transform), "localScale.x",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.12f, 1.08f),
                new Keyframe(0.24f, 1f)
            ));
        idleClip.SetCurve("", typeof(Transform), "localScale.y",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.12f, 0.88f),
                new Keyframe(0.24f, 1f)
            ));
        var idleSettings = AnimationUtility.GetAnimationClipSettings(idleClip);
        idleSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(idleClip, idleSettings);
        AssetDatabase.CreateAsset(idleClip, "Assets/Animations/Idle.anim");

        // ── Flap clip: sharp upward pulse ──
        var flapClip = new AnimationClip { name = "Flap" };
        flapClip.SetCurve("", typeof(Transform), "localScale.x",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.05f, 0.8f),
                new Keyframe(0.15f, 1.1f),
                new Keyframe(0.25f, 1f)
            ));
        flapClip.SetCurve("", typeof(Transform), "localScale.y",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.05f, 1.25f),
                new Keyframe(0.15f, 0.85f),
                new Keyframe(0.25f, 1f)
            ));
        AssetDatabase.CreateAsset(flapClip, "Assets/Animations/Flap.anim");

        // ── Animator Controller ──
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Flap", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var rootSM = controller.layers[0].stateMachine;

        var idleState = rootSM.AddState("Idle");
        idleState.motion = idleClip;
        rootSM.defaultState = idleState;

        var flapState = rootSM.AddState("Flap");
        flapState.motion = flapClip;

        // Any State → Flap (on trigger)
        var toFlap = rootSM.AddAnyStateTransition(flapState);
        toFlap.AddCondition(AnimatorConditionMode.If, 0, "Flap");
        toFlap.duration = 0f;
        toFlap.hasExitTime = false;

        // Flap → Idle (after clip finishes)
        var toIdle = flapState.AddTransition(idleState);
        toIdle.hasExitTime = true;
        toIdle.exitTime = 1f;
        toIdle.duration = 0.1f;

        AssetDatabase.SaveAssets();
        Debug.Log("[GameSetup] Created Kookaburra animator with Idle + Flap clips");
        return controller;
    }

    // ─── Scene Setup ──────────────────────────────────────────────

    private static void SetupScene(
        BirdStats birdStats,
        DifficultyConfig difficultyConfig,
        AudioConfig audioConfig,
        ObstaclePair obstaclePairPrefab,
        ParticleSystem scoreParticlePrefab,
        ParticleSystem deathParticlePrefab)
    {
        // ── Camera ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // sky blue
            cam.transform.position = new Vector3(0, 0, -10);

            if (cam.GetComponent<ScreenShake>() == null)
            {
                cam.gameObject.AddComponent<ScreenShake>();
                Debug.Log("[GameSetup] Added ScreenShake to Camera");
            }
        }

        // ── Managers ──
        CreateManager<GameManager>("GameManager");
        CreateManager<ScoreManager>("ScoreManager");

        var diffMgr = CreateManager<DifficultyManager>("DifficultyManager");
        WireField(diffMgr, "_config", difficultyConfig);

        var audioMgr = CreateManager<AudioManager>("AudioManager");
        WireField(audioMgr, "_audioConfig", audioConfig);
        // Add AudioSources
        var audioGO = audioMgr.gameObject;
        var audioSources = audioGO.GetComponents<AudioSource>();
        AudioSource sfxSource, musicSource;
        if (audioSources.Length < 2)
        {
            sfxSource = audioSources.Length >= 1 ? audioSources[0] : audioGO.AddComponent<AudioSource>();
            musicSource = audioGO.AddComponent<AudioSource>();
        }
        else
        {
            sfxSource = audioSources[0];
            musicSource = audioSources[1];
        }
        sfxSource.playOnAwake = false;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        WireField(audioMgr, "_sfxSource", sfxSource);
        WireField(audioMgr, "_musicSource", musicSource);

        CreateManager<FrameRateManager>("FrameRateManager");

        // ── Player ──
        var kookaburaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Kookaburra.prefab");
        GameObject player = GameObject.Find("Kookaburra");
        if (player == null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(kookaburaPrefab);
            player.name = "Kookaburra";
        }
        player.transform.position = new Vector3(-2f, 0f, 0f);

        // ── Obstacle Spawner ──
        var spawner = CreateManager<ObstacleSpawner>("ObstacleSpawner");
        WireField(spawner, "_difficultyConfig", difficultyConfig);
        WireField(spawner, "_obstaclePairPrefab", obstaclePairPrefab);

        // ── Particle Spawner ──
        var particleSpawner = CreateManager<ParticleSpawner>("ParticleSpawner");
        WireField(particleSpawner, "_scoreParticlePrefab", scoreParticlePrefab);
        WireField(particleSpawner, "_deathParticlePrefab", deathParticlePrefab);
        WireField(particleSpawner, "_playerTransform", player.transform);

        // ── World Boundaries (ground + ceiling colliders) ──
        CreateBoundary("Ground", new Vector3(0f, -6f, 0f));
        CreateBoundary("Ceiling", new Vector3(0f, 6f, 0f));

        // ── Background Parallax ──
        CreateParallaxLayer("BackgroundFar", 0.3f, new Color(0.6f, 0.85f, 0.95f), -2);
        CreateParallaxLayer("BackgroundNear", 0.6f, new Color(0.4f, 0.75f, 0.4f), -1);

        // ── UI ──
        SetupUI();

        Debug.Log("[GameSetup] Scene setup complete!");
    }

    private static T CreateManager<T>(string name) where T : MonoBehaviour
    {
        var existing = Object.FindFirstObjectByType<T>();
        if (existing != null) return existing;

        var go = new GameObject(name);
        var comp = go.AddComponent<T>();
        Debug.Log($"[GameSetup] Created {name}");
        return comp;
    }

    private static void WireField(Component comp, string fieldName, Object value)
    {
        var so = new SerializedObject(comp);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"[GameSetup] Could not find field '{fieldName}' on {comp.GetType().Name}");
        }
    }

    private static void CreateParallaxLayer(string name, float speedMultiplier, Color color, int sortOrder)
    {
        if (GameObject.Find(name) != null) return;

        var bgSprite = LoadOrCreateSprite($"Background/{name.ToLower()}", color, 128, 128, pixelsPerUnit: 64);

        var go = new GameObject(name);
        var layer = go.AddComponent<ParallaxLayer>();

        // Sprite A
        var sprA = new GameObject("SpriteA");
        sprA.transform.SetParent(go.transform);
        sprA.transform.localPosition = Vector3.zero;
        var srA = sprA.AddComponent<SpriteRenderer>();
        srA.sprite = bgSprite;
        srA.sortingLayerName = "Background";
        srA.sortingOrder = sortOrder;
        srA.drawMode = SpriteDrawMode.Tiled;
        srA.size = new Vector2(20f, 10f);

        // Sprite B
        var sprB = new GameObject("SpriteB");
        sprB.transform.SetParent(go.transform);
        sprB.transform.localPosition = new Vector3(20f, 0, 0);
        var srB = sprB.AddComponent<SpriteRenderer>();
        srB.sprite = bgSprite;
        srB.sortingLayerName = "Background";
        srB.sortingOrder = sortOrder;
        srB.drawMode = SpriteDrawMode.Tiled;
        srB.size = new Vector2(20f, 10f);

        // Wire
        var so = new SerializedObject(layer);
        so.FindProperty("_scrollSpeedMultiplier").floatValue = speedMultiplier;
        so.FindProperty("_spriteA").objectReferenceValue = srA;
        so.FindProperty("_spriteB").objectReferenceValue = srB;
        so.FindProperty("_baseSpeed").floatValue = 3f;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[GameSetup] Created parallax layer: {name}");
    }

    private static void CreateBoundary(string name, Vector3 position)
    {
        if (GameObject.Find(name) != null) return;

        var go = new GameObject(name);
        go.transform.position = position;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(40f, 1f); // wide enough to span any screen width
        Debug.Log($"[GameSetup] Created boundary: {name} at y={position.y}");
    }

    private static void SetupUI()
    {
        if (Object.FindFirstObjectByType<UIManager>() != null) return;

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingLayerName = "UI";
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Main Menu Panel ──
        var menuPanel = CreatePanel(canvasGO.transform, "MainMenuPanel");
        var titleText = CreateTMPText(menuPanel.transform, "TitleText", "FlappyKookaburra", 72);
        SetRectTransform(titleText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(800, 100));
        var startBtn = CreateButton(menuPanel.transform, "StartButton", "TAP TO START");
        SetRectTransform(startBtn, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(400, 80));

        // ── HUD Panel ──
        var hudPanel = CreatePanel(canvasGO.transform, "HUDPanel");
        hudPanel.SetActive(false);
        var scoreText = CreateTMPText(hudPanel.transform, "ScoreText", "0", 96);
        SetRectTransform(scoreText, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(400, 120));
        var punchEffect = scoreText.AddComponent<ScorePunchEffect>();

        // ── Game Over Panel ──
        var gameOverPanel = CreatePanel(canvasGO.transform, "GameOverPanel");
        gameOverPanel.SetActive(false);
        var canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        var gameOverText = CreateTMPText(gameOverPanel.transform, "GameOverText", "GAME OVER", 64);
        SetRectTransform(gameOverText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(800, 100));

        var finalScoreText = CreateTMPText(gameOverPanel.transform, "FinalScoreText", "Score: 0", 48);
        SetRectTransform(finalScoreText, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(600, 80));

        var highScoreText = CreateTMPText(gameOverPanel.transform, "HighScoreText", "Best: 0", 36);
        SetRectTransform(highScoreText, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(600, 60));

        var newHighScoreIndicator = CreateTMPText(gameOverPanel.transform, "NewHighScoreIndicator", "NEW HIGH SCORE!", 40);
        SetRectTransform(newHighScoreIndicator, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), Vector2.zero, new Vector2(600, 60));
        newHighScoreIndicator.SetActive(false);

        var restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "RESTART");
        SetRectTransform(restartBtn, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(400, 80));

        // ── EventSystem ──
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Wire UIManager ──
        var uiMgr = canvasGO.AddComponent<UIManager>();
        var uiSo = new SerializedObject(uiMgr);
        uiSo.FindProperty("_mainMenuPanel").objectReferenceValue = menuPanel;
        uiSo.FindProperty("_hudPanel").objectReferenceValue = hudPanel;
        uiSo.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSo.FindProperty("_scoreText").objectReferenceValue = scoreText.GetComponent<TMP_Text>();
        uiSo.FindProperty("_finalScoreText").objectReferenceValue = finalScoreText.GetComponent<TMP_Text>();
        uiSo.FindProperty("_highScoreText").objectReferenceValue = highScoreText.GetComponent<TMP_Text>();
        uiSo.FindProperty("_newHighScoreIndicator").objectReferenceValue = newHighScoreIndicator;
        uiSo.FindProperty("_startButton").objectReferenceValue = startBtn.GetComponentInChildren<Button>();
        uiSo.FindProperty("_restartButton").objectReferenceValue = restartBtn.GetComponentInChildren<Button>();
        uiSo.FindProperty("_gameOverCanvasGroup").objectReferenceValue = canvasGroup;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created UI hierarchy");
    }

    // ─── UI Helpers ───────────────────────────────────────────────

    private static GameObject CreatePanel(Transform parent, string name)
    {
        var panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        // Use default TMP font (will resolve at runtime if TMP essentials are imported)
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        var btn = go.AddComponent<Button>();

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go;
    }

    private static void SetRectTransform(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
    }

    // ─── Utility ──────────────────────────────────────────────────

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
