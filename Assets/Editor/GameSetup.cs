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

        string[] assetPaths = {
            "Assets/Prefabs/ObstaclePair.prefab",
            "Assets/Prefabs/Kookaburra.prefab",
            "Assets/Prefabs/ScoreParticle.prefab",
            "Assets/Prefabs/DeathParticle.prefab",
            "Assets/Animations/KookaburraAnimator.controller",
            "Assets/Animations/Idle.anim",
            "Assets/Animations/Flap.anim",
            "Assets/Animations/Glide.anim",
            "Assets/Animations/Die.anim",
            "Assets/Animations/Blink.anim"
        };
        foreach (var path in assetPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[GameSetup] Deleted {path}");
            }
        }

        var scenePlayer = GameObject.Find("Kookaburra");
        if (scenePlayer != null)
        {
            Object.DestroyImmediate(scenePlayer);
            Debug.Log("[GameSetup] Removed old Kookaburra scene instance");
        }

        AssetDatabase.Refresh();
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
        EnsureFolder("Assets/Data/UI");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Sprites");
        EnsureFolder("Assets/Sprites/Kookaburra");
        EnsureFolder("Assets/Sprites/Background");
        EnsureFolder("Assets/Sprites/Obstacles");
        EnsureFolder("Assets/Sprites/Ground");
        EnsureFolder("Assets/Sprites/UI");
        EnsureFolder("Assets/Audio");
        EnsureFolder("Assets/Animations");
        EnsureFolder("Assets/UI");
        EnsureFolder("Assets/UI/Fonts");
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
        so.minimumGapSize = 2.0f;
        so.gapSizeCurve = CreateSCurve();
        so.initialScrollSpeed = 3f;
        so.maximumScrollSpeed = 8f;
        so.scrollSpeedCurve = CreateSCurve();
        so.spawnInterval = 1.5f;
        so.minimumSpawnInterval = 0.6f;
        so.spawnIntervalCurve = CreateSCurve();
        so.initialGapYPadding = 3f;
        so.minimumGapYPadding = 1.5f;
        so.gapYVarianceCurve = CreateSCurve();
        so.maxSpeedVariance = 0.15f;
        so.speedVarianceCurve = CreateSCurve();
        so.maxOscillationAmplitude = 1.2f;
        so.oscillationFrequency = 1.0f;
        so.oscillationAmplitudeCurve = CreateOscillationCurve();
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

    private enum SpriteShape { Rectangle, Ellipse, RoundedRect }

    private static Sprite LoadOrCreateSprite(string name, Color fallbackColor, int fallbackWidth, int fallbackHeight, int pixelsPerUnit = 64, bool alphaIsTransparency = false, SpriteShape shape = SpriteShape.Rectangle)
    {
        string path = $"Assets/Sprites/{name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (existing == null)
        {
            var tex = new Texture2D(fallbackWidth, fallbackHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[fallbackWidth * fallbackHeight];

            if (alphaIsTransparency && shape != SpriteShape.Rectangle)
            {
                // Fill with transparent first
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

                float cx = fallbackWidth * 0.5f;
                float cy = fallbackHeight * 0.5f;
                float rx = fallbackWidth * 0.5f;
                float ry = fallbackHeight * 0.5f;

                for (int y = 0; y < fallbackHeight; y++)
                {
                    for (int x = 0; x < fallbackWidth; x++)
                    {
                        bool inside = false;
                        if (shape == SpriteShape.Ellipse)
                        {
                            float dx = (x + 0.5f - cx) / rx;
                            float dy = (y + 0.5f - cy) / ry;
                            inside = (dx * dx + dy * dy) <= 1f;
                        }
                        else if (shape == SpriteShape.RoundedRect)
                        {
                            float cornerRadius = Mathf.Min(fallbackWidth, fallbackHeight) * 0.25f;
                            inside = IsInsideRoundedRect(x, y, fallbackWidth, fallbackHeight, cornerRadius);
                        }

                        if (inside)
                        {
                            // Simple shading: slightly lighter at top, darker at bottom
                            float shade = 0.85f + 0.15f * ((float)y / fallbackHeight);
                            pixels[y * fallbackWidth + x] = new Color(
                                fallbackColor.r * shade,
                                fallbackColor.g * shade,
                                fallbackColor.b * shade,
                                fallbackColor.a);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < pixels.Length; i++) pixels[i] = fallbackColor;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = alphaIsTransparency;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadOrCreateSpriteWithPivot(string name, Color fallbackColor, int fallbackWidth, int fallbackHeight, int pixelsPerUnit, Vector2 pivot, SpriteShape shape = SpriteShape.Ellipse)
    {
        string path = $"Assets/Sprites/{name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (existing == null)
        {
            var tex = new Texture2D(fallbackWidth, fallbackHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[fallbackWidth * fallbackHeight];

            // Fill with transparent, then draw shape
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            float cx = fallbackWidth * 0.5f;
            float cy = fallbackHeight * 0.5f;
            float rx = fallbackWidth * 0.5f;
            float ry = fallbackHeight * 0.5f;

            for (int y = 0; y < fallbackHeight; y++)
            {
                for (int x = 0; x < fallbackWidth; x++)
                {
                    bool inside = false;
                    if (shape == SpriteShape.Ellipse)
                    {
                        float dx = (x + 0.5f - cx) / rx;
                        float dy = (y + 0.5f - cy) / ry;
                        inside = (dx * dx + dy * dy) <= 1f;
                    }
                    else if (shape == SpriteShape.RoundedRect)
                    {
                        float cornerRadius = Mathf.Min(fallbackWidth, fallbackHeight) * 0.25f;
                        inside = IsInsideRoundedRect(x, y, fallbackWidth, fallbackHeight, cornerRadius);
                    }
                    else
                    {
                        inside = true;
                    }

                    if (inside)
                    {
                        float shade = 0.85f + 0.15f * ((float)y / fallbackHeight);
                        pixels[y * fallbackWidth + x] = new Color(
                            fallbackColor.r * shade,
                            fallbackColor.g * shade,
                            fallbackColor.b * shade,
                            fallbackColor.a);
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            importer.SetTextureSettings(settings);
            importer.spritePivot = pivot;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static bool IsInsideRoundedRect(int x, int y, int w, int h, float r)
    {
        // Check if point is inside a rounded rectangle
        if (x >= r && x < w - r) return true; // Middle band
        if (y >= r && y < h - r) return true; // Middle band vertical

        // Check corners
        float cx = x < r ? r : w - r;
        float cy = y < r ? r : h - r;
        float dx = x - cx;
        float dy = y - cy;
        return (dx * dx + dy * dy) <= r * r;
    }

    // ─── Prefabs ──────────────────────────────────────────────────

    private static ObstaclePair CreateObstaclePairPrefab()
    {
        const string path = "Assets/Prefabs/ObstaclePair.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<ObstaclePair>(path);
        if (existing != null) return existing;

        // Trunk segment — tileable brown bark texture
        // AI Art Prompt: Pixel art eucalyptus bark segment, tileable vertically. Brown/grey bark texture.
        // Transparent sides. 64×64px.
        var trunkSprite = LoadOrCreateSprite("Obstacles/trunk-segment", new Color(0.35f, 0.25f, 0.15f), 64, 64, pixelsPerUnit: 64, alphaIsTransparency: true);

        // Leaf cap top — green leafy cap for top of pipe
        // AI Art Prompt: Pixel art eucalyptus leaf cluster cap, facing downward. Green leaves with brown branch.
        // Transparent background. 128×64px.
        var capTopSprite = LoadOrCreateSprite("Obstacles/trunk-cap-top", new Color(0.2f, 0.55f, 0.2f), 128, 64, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.RoundedRect);

        // Root cap bottom — brown root cap for bottom of pipe
        // AI Art Prompt: Pixel art tree root/base cap, facing upward. Brown roots spreading.
        // Transparent background. 128×64px.
        var capBottomSprite = LoadOrCreateSprite("Obstacles/trunk-cap-bottom", new Color(0.4f, 0.3f, 0.15f), 128, 64, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.RoundedRect);

        const float targetPipeHeight = 20f;
        const float targetPipeWidth = 1.5f;

        // Root
        var root = new GameObject("ObstaclePair");
        var obstacle = root.AddComponent<Obstacle>();
        var pair = root.AddComponent<ObstaclePair>();

        // ── Top pipe (trunk + cap at bottom edge facing gap) ──
        var top = new GameObject("TopPipe");
        top.transform.SetParent(root.transform);
        top.transform.localPosition = new Vector3(0, 5f, 0);

        // Trunk (tiled)
        var topTrunk = new GameObject("Trunk");
        topTrunk.transform.SetParent(top.transform);
        topTrunk.transform.localPosition = Vector3.zero;
        var topTrunkSr = topTrunk.AddComponent<SpriteRenderer>();
        topTrunkSr.sprite = trunkSprite;
        topTrunkSr.drawMode = SpriteDrawMode.Tiled;
        topTrunkSr.size = new Vector2(targetPipeWidth, targetPipeHeight);
        topTrunkSr.sortingLayerName = "Obstacles";
        topTrunkSr.sortingOrder = 0;

        // Cap (at the gap edge)
        var topCap = new GameObject("Cap");
        topCap.transform.SetParent(top.transform);
        topCap.transform.localPosition = new Vector3(0, -targetPipeHeight / 2f, 0);
        var topCapSr = topCap.AddComponent<SpriteRenderer>();
        topCapSr.sprite = capTopSprite;
        topCapSr.sortingLayerName = "Obstacles";
        topCapSr.sortingOrder = 1;

        var topCol = top.AddComponent<BoxCollider2D>();
        topCol.size = new Vector2(targetPipeWidth, targetPipeHeight);

        // ── Bottom pipe (trunk + cap at top edge facing gap) ──
        var bottom = new GameObject("BottomPipe");
        bottom.transform.SetParent(root.transform);
        bottom.transform.localPosition = new Vector3(0, -5f, 0);

        // Trunk (tiled)
        var bottomTrunk = new GameObject("Trunk");
        bottomTrunk.transform.SetParent(bottom.transform);
        bottomTrunk.transform.localPosition = Vector3.zero;
        var bottomTrunkSr = bottomTrunk.AddComponent<SpriteRenderer>();
        bottomTrunkSr.sprite = trunkSprite;
        bottomTrunkSr.drawMode = SpriteDrawMode.Tiled;
        bottomTrunkSr.size = new Vector2(targetPipeWidth, targetPipeHeight);
        bottomTrunkSr.sortingLayerName = "Obstacles";
        bottomTrunkSr.sortingOrder = 0;

        // Cap (at the gap edge)
        var bottomCap = new GameObject("Cap");
        bottomCap.transform.SetParent(bottom.transform);
        bottomCap.transform.localPosition = new Vector3(0, targetPipeHeight / 2f, 0);
        var bottomCapSr = bottomCap.AddComponent<SpriteRenderer>();
        bottomCapSr.sprite = capBottomSprite;
        bottomCapSr.sortingLayerName = "Obstacles";
        bottomCapSr.sortingOrder = 1;

        var bottomCol = bottom.AddComponent<BoxCollider2D>();
        bottomCol.size = new Vector2(targetPipeWidth, targetPipeHeight);

        Debug.Log("[GameSetup] Pipe prefab uses modular trunk-segment (tiled) + leaf/root caps");

        // Score zone (trigger between pipes)
        var zone = new GameObject("ScoreZone");
        zone.transform.SetParent(root.transform);
        zone.transform.localPosition = Vector3.zero;
        var zoneCol = zone.AddComponent<BoxCollider2D>();
        zoneCol.isTrigger = true;
        zoneCol.size = new Vector2(0.5f, 4f);
        var scoreZone = zone.AddComponent<ScoreZone>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        var so = new SerializedObject(prefab.GetComponent<ObstaclePair>());
        so.FindProperty("_topPipe").objectReferenceValue = prefab.transform.Find("TopPipe");
        so.FindProperty("_bottomPipe").objectReferenceValue = prefab.transform.Find("BottomPipe");
        so.FindProperty("_scoreZone").objectReferenceValue = prefab.transform.Find("ScoreZone").GetComponent<ScoreZone>();
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created modular ObstaclePair prefab with trunk + caps");
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
        main.startColor = new Color(1f, 0.85f, 0f);
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
        // Brown/tan feather burst instead of red
        main.startColor = new Color(0.65f, 0.5f, 0.3f);
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
        Debug.Log("[GameSetup] Created DeathParticle prefab (feather burst)");
        return prefab.GetComponent<ParticleSystem>();
    }

    private static GameObject CreateKookaburaPrefab(BirdStats stats)
    {
        const string path = "Assets/Prefabs/Kookaburra.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        // ── Multi-part bird rig ──
        // AI Art Prompt: Pixel art kookaburra body (no wings, no tail), side profile facing right.
        // Brown/tan body, cream-white chest, dark beak. Transparent background. 32×32px.
        var bodySprite = LoadOrCreateSprite("Kookaburra/kookaburra-body",
            new Color(0.6f, 0.4f, 0.2f), 32, 32, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.Ellipse);

        // AI Art Prompt: Pixel art kookaburra wing, side view. Brown/dark feather pattern.
        // Pivot at shoulder (top-left). Transparent background. 16×24px.
        var wingSprite = LoadOrCreateSpriteWithPivot("Kookaburra/kookaburra-wing",
            new Color(0.45f, 0.3f, 0.15f), 16, 24, 64, new Vector2(0.3f, 0.8f));

        // AI Art Prompt: Pixel art kookaburra tail feathers, horizontal spread.
        // Brown with dark tips. Transparent background. 16×8px.
        var tailSprite = LoadOrCreateSprite("Kookaburra/kookaburra-tail",
            new Color(0.5f, 0.35f, 0.2f), 16, 8, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.Ellipse);

        // AI Art Prompt: Pixel art kookaburra eye, simple black dot with white highlight.
        // Transparent background. 8×8px.
        var eyeSprite = LoadOrCreateSprite("Kookaburra/kookaburra-eye",
            new Color(0.1f, 0.1f, 0.1f), 8, 8, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.Ellipse);

        var go = new GameObject("Kookaburra");
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Default");

        // Rigidbody & Collider on root
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = stats.gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        // ── Body (sortingOrder 1) ──
        var body = new GameObject("Body");
        body.transform.SetParent(go.transform);
        body.transform.localPosition = Vector3.zero;
        var bodySr = body.AddComponent<SpriteRenderer>();
        bodySr.sprite = bodySprite;
        bodySr.sortingLayerName = "Player";
        bodySr.sortingOrder = 1;

        // ── Left Wing (sortingOrder 0, behind body, pivot at shoulder) ──
        var leftWing = new GameObject("LeftWing");
        leftWing.transform.SetParent(go.transform);
        leftWing.transform.localPosition = new Vector3(-0.05f, 0.1f, 0f);
        var leftWingSr = leftWing.AddComponent<SpriteRenderer>();
        leftWingSr.sprite = wingSprite;
        leftWingSr.sortingLayerName = "Player";
        leftWingSr.sortingOrder = 0;

        // ── Right Wing (sortingOrder 2, in front of body, pivot at shoulder) ──
        var rightWing = new GameObject("RightWing");
        rightWing.transform.SetParent(go.transform);
        rightWing.transform.localPosition = new Vector3(-0.05f, 0.1f, 0f);
        var rightWingSr = rightWing.AddComponent<SpriteRenderer>();
        rightWingSr.sprite = wingSprite;
        rightWingSr.sortingLayerName = "Player";
        rightWingSr.sortingOrder = 2;

        // ── Tail (sortingOrder 0, behind body) ──
        var tail = new GameObject("Tail");
        tail.transform.SetParent(go.transform);
        tail.transform.localPosition = new Vector3(-0.25f, -0.05f, 0f);
        var tailSr = tail.AddComponent<SpriteRenderer>();
        tailSr.sprite = tailSprite;
        tailSr.sortingLayerName = "Player";
        tailSr.sortingOrder = 0;

        // ── Eye (sortingOrder 3, in front of everything) ──
        var eye = new GameObject("Eye");
        eye.transform.SetParent(go.transform);
        eye.transform.localPosition = new Vector3(0.15f, 0.1f, 0f);
        var eyeSr = eye.AddComponent<SpriteRenderer>();
        eyeSr.sprite = eyeSprite;
        eyeSr.sortingLayerName = "Player";
        eyeSr.sortingOrder = 3;

        // Components
        var animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = CreateKookaburraAnimator();

        var pc = go.AddComponent<PlayerController>();
        var rig = go.AddComponent<KookaburraRig>();

        // Save prefab
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // Wire PlayerController fields
        var pcSo = new SerializedObject(prefab.GetComponent<PlayerController>());
        pcSo.FindProperty("_birdStats").objectReferenceValue = stats;
        pcSo.FindProperty("_animator").objectReferenceValue = prefab.GetComponent<Animator>();
        pcSo.ApplyModifiedPropertiesWithoutUndo();

        // Wire KookaburraRig fields
        var rigSo = new SerializedObject(prefab.GetComponent<KookaburraRig>());
        rigSo.FindProperty("_body").objectReferenceValue = prefab.transform.Find("Body");
        rigSo.FindProperty("_leftWing").objectReferenceValue = prefab.transform.Find("LeftWing");
        rigSo.FindProperty("_rightWing").objectReferenceValue = prefab.transform.Find("RightWing");
        rigSo.FindProperty("_tail").objectReferenceValue = prefab.transform.Find("Tail");
        rigSo.FindProperty("_eye").objectReferenceValue = prefab.transform.Find("Eye");
        rigSo.FindProperty("_bodyRenderer").objectReferenceValue = prefab.transform.Find("Body").GetComponent<SpriteRenderer>();
        rigSo.FindProperty("_leftWingRenderer").objectReferenceValue = prefab.transform.Find("LeftWing").GetComponent<SpriteRenderer>();
        rigSo.FindProperty("_rightWingRenderer").objectReferenceValue = prefab.transform.Find("RightWing").GetComponent<SpriteRenderer>();
        rigSo.FindProperty("_tailRenderer").objectReferenceValue = prefab.transform.Find("Tail").GetComponent<SpriteRenderer>();
        rigSo.FindProperty("_eyeRenderer").objectReferenceValue = prefab.transform.Find("Eye").GetComponent<SpriteRenderer>();
        rigSo.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created multi-part Kookaburra prefab (body, 2 wings, tail, eye)");
        return prefab;
    }

    private static AnimatorController CreateKookaburraAnimator()
    {
        const string controllerPath = "Assets/Animations/KookaburraAnimator.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (existing != null) return existing;

        EnsureFolder("Assets/Animations");

        // ── Idle clip: wing oscillation via rotation ──
        var idleClip = new AnimationClip { name = "Idle" };
        // Left wing: oscillate rotation +-15 degrees
        idleClip.SetCurve("LeftWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, 15f),
                new Keyframe(0.3f, 0f),
                new Keyframe(0.45f, -15f),
                new Keyframe(0.6f, 0f)
            ));
        // Right wing: oscillate opposite
        idleClip.SetCurve("RightWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, -15f),
                new Keyframe(0.3f, 0f),
                new Keyframe(0.45f, 15f),
                new Keyframe(0.6f, 0f)
            ));
        // Tail gentle wag
        idleClip.SetCurve("Tail", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.3f, 5f),
                new Keyframe(0.6f, 0f)
            ));
        var idleSettings = AnimationUtility.GetAnimationClipSettings(idleClip);
        idleSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(idleClip, idleSettings);
        AssetDatabase.CreateAsset(idleClip, "Assets/Animations/Idle.anim");

        // ── Flap clip: sharp upstroke ──
        var flapClip = new AnimationClip { name = "Flap" };
        // Left wing: thrust to +30 degrees (upstroke) then return
        flapClip.SetCurve("LeftWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.05f, 30f),
                new Keyframe(0.15f, -10f),
                new Keyframe(0.25f, 0f)
            ));
        // Right wing: opposite
        flapClip.SetCurve("RightWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.05f, -30f),
                new Keyframe(0.15f, 10f),
                new Keyframe(0.25f, 0f)
            ));
        AssetDatabase.CreateAsset(flapClip, "Assets/Animations/Flap.anim");

        // ── Glide clip: wings held extended ──
        var glideClip = new AnimationClip { name = "Glide" };
        glideClip.SetCurve("LeftWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.1f, 10f)
            ));
        glideClip.SetCurve("RightWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.1f, -10f)
            ));
        var glideSettings = AnimationUtility.GetAnimationClipSettings(glideClip);
        glideSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(glideClip, glideSettings);
        AssetDatabase.CreateAsset(glideClip, "Assets/Animations/Glide.anim");

        // ── Die clip: wings droop, eye closes ──
        var dieClip = new AnimationClip { name = "Die" };
        dieClip.SetCurve("LeftWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, -45f)
            ));
        dieClip.SetCurve("RightWing", typeof(Transform), "localEulerAngles.z",
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 45f)
            ));
        // Eye shrinks (simulating closing)
        dieClip.SetCurve("Eye", typeof(Transform), "localScale.y",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.15f, 0.1f)
            ));
        AssetDatabase.CreateAsset(dieClip, "Assets/Animations/Die.anim");

        // ── Blink clip: quick eye close/open ──
        var blinkClip = new AnimationClip { name = "Blink" };
        blinkClip.SetCurve("Eye", typeof(Transform), "localScale.y",
            new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.05f, 0.1f),
                new Keyframe(0.1f, 1f)
            ));
        AssetDatabase.CreateAsset(blinkClip, "Assets/Animations/Blink.anim");

        // ── Animator Controller ──
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Flap", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Glide", AnimatorControllerParameterType.Bool);

        var rootSM = controller.layers[0].stateMachine;

        // States
        var idleState = rootSM.AddState("Idle");
        idleState.motion = idleClip;
        rootSM.defaultState = idleState;

        var flapState = rootSM.AddState("Flap");
        flapState.motion = flapClip;

        var glideState = rootSM.AddState("Glide");
        glideState.motion = glideClip;

        var dieState = rootSM.AddState("Die");
        dieState.motion = dieClip;

        // Transitions
        // Any State → Flap (trigger)
        var toFlap = rootSM.AddAnyStateTransition(flapState);
        toFlap.AddCondition(AnimatorConditionMode.If, 0, "Flap");
        toFlap.duration = 0f;
        toFlap.hasExitTime = false;

        // Flap → Idle (after clip finishes)
        var flapToIdle = flapState.AddTransition(idleState);
        flapToIdle.hasExitTime = true;
        flapToIdle.exitTime = 1f;
        flapToIdle.duration = 0.1f;

        // Idle → Glide (when Glide bool is true)
        var idleToGlide = idleState.AddTransition(glideState);
        idleToGlide.AddCondition(AnimatorConditionMode.If, 0, "Glide");
        idleToGlide.duration = 0.15f;
        idleToGlide.hasExitTime = false;

        // Glide → Idle (when Glide bool is false)
        var glideToIdle = glideState.AddTransition(idleState);
        glideToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Glide");
        glideToIdle.duration = 0.15f;
        glideToIdle.hasExitTime = false;

        // Any State → Die (trigger)
        var toDie = rootSM.AddAnyStateTransition(dieState);
        toDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
        toDie.duration = 0f;
        toDie.hasExitTime = false;

        // Add Blink sub-layer
        controller.AddLayer("Blink");
        var blinkLayer = controller.layers[1];
        blinkLayer.defaultWeight = 1f;
        var blinkSM = blinkLayer.stateMachine;

        var blinkIdle = blinkSM.AddState("BlinkIdle");
        blinkIdle.motion = null; // Empty state — no animation
        blinkSM.defaultState = blinkIdle;

        var blinkState = blinkSM.AddState("Blink");
        blinkState.motion = blinkClip;

        // Random blink transition (using exit time to simulate random intervals)
        var toBlinkState = blinkIdle.AddTransition(blinkState);
        toBlinkState.hasExitTime = true;
        toBlinkState.exitTime = 1f;
        toBlinkState.duration = 0f;

        var fromBlinkState = blinkState.AddTransition(blinkIdle);
        fromBlinkState.hasExitTime = true;
        fromBlinkState.exitTime = 1f;
        fromBlinkState.duration = 0f;

        // Update the layer array (Unity requires setting it back)
        var layers = controller.layers;
        layers[1] = blinkLayer;
        controller.layers = layers;

        AssetDatabase.SaveAssets();
        Debug.Log("[GameSetup] Created Kookaburra animator with Idle, Flap, Glide, Die + Blink sub-layer");
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
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
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

        // ── World Boundaries ──
        CreateBoundary("Ground", new Vector3(0f, -6f, 0f));
        CreateBoundary("Ceiling", new Vector3(0f, 6f, 0f));

        // ── Background Parallax (4+ layers) ──
        // Layer 1: Sky gradient (speed 0.0 — static backdrop)
        CreateParallaxLayer("SkyGradient", 0.0f, new Color(0.53f, 0.81f, 0.92f), -4, "Background/sky-gradient");
        // Layer 2: Hills silhouette (speed 0.2)
        CreateParallaxLayer("HillsSilhouette", 0.2f, new Color(0.35f, 0.55f, 0.35f), -3, "Background/hills-silhouette");
        // Layer 3: Trees midground (speed 0.5)
        CreateParallaxLayer("TreesMidground", 0.5f, new Color(0.3f, 0.65f, 0.3f), -2, "Background/trees-midground");
        // Layer 4: Near trees (speed 0.7)
        CreateParallaxLayer("BackgroundNear", 0.7f, new Color(0.4f, 0.75f, 0.4f), -1, "Background/backgroundnear");

        // ── Ground Layer ──
        CreateGroundLayer();

        // ── Cloud Spawner ──
        CreateCloudSpawner();

        // ── Difficulty Visual Feedback ──
        SetupDifficultyVisualFeedback(cam);

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

    private static void CreateParallaxLayer(string name, float speedMultiplier, Color color, int sortOrder, string spritePath = null)
    {
        if (GameObject.Find(name) != null) return;

        bool isTransparent = !name.Contains("Sky");
        var bgSprite = LoadOrCreateSprite(
            spritePath ?? $"Background/{name.ToLower()}",
            color, 128, 128, pixelsPerUnit: 64, alphaIsTransparency: isTransparent);

        var go = new GameObject(name);
        var layer = go.AddComponent<ParallaxLayer>();

        var sprA = new GameObject("SpriteA");
        sprA.transform.SetParent(go.transform);
        sprA.transform.localPosition = Vector3.zero;
        var srA = sprA.AddComponent<SpriteRenderer>();
        srA.sprite = bgSprite;
        srA.sortingLayerName = "Background";
        srA.sortingOrder = sortOrder;
        srA.drawMode = SpriteDrawMode.Tiled;
        srA.size = new Vector2(20f, 10f);

        var sprB = new GameObject("SpriteB");
        sprB.transform.SetParent(go.transform);
        sprB.transform.localPosition = new Vector3(20f, 0, 0);
        var srB = sprB.AddComponent<SpriteRenderer>();
        srB.sprite = bgSprite;
        srB.sortingLayerName = "Background";
        srB.sortingOrder = sortOrder;
        srB.drawMode = SpriteDrawMode.Tiled;
        srB.size = new Vector2(20f, 10f);

        var so = new SerializedObject(layer);
        so.FindProperty("_scrollSpeedMultiplier").floatValue = speedMultiplier;
        so.FindProperty("_spriteA").objectReferenceValue = srA;
        so.FindProperty("_spriteB").objectReferenceValue = srB;
        so.FindProperty("_baseSpeed").floatValue = 3f;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[GameSetup] Created parallax layer: {name} (speed: {speedMultiplier})");
    }

    private static void CreateGroundLayer()
    {
        if (GameObject.Find("Ground Layer") != null) return;

        // Surface detail sprite
        // AI Art Prompt: Pixel art Australian ground surface. Grass tufts, small rocks on red-brown dirt.
        // Transparent below surface line. Tileable horizontally. 128×32px.
        var surfaceSprite = LoadOrCreateSprite("Ground/ground-surface", new Color(0.55f, 0.3f, 0.15f), 128, 32, pixelsPerUnit: 64, alphaIsTransparency: true);

        var go = new GameObject("Ground Layer");
        go.transform.position = new Vector3(0f, -4.5f, 0f);
        var layer = go.AddComponent<ParallaxLayer>();

        var sprA = new GameObject("SpriteA");
        sprA.transform.SetParent(go.transform);
        sprA.transform.localPosition = Vector3.zero;
        var srA = sprA.AddComponent<SpriteRenderer>();
        srA.sprite = surfaceSprite;
        srA.sortingLayerName = "Ground";
        srA.sortingOrder = 0;
        srA.drawMode = SpriteDrawMode.Tiled;
        srA.size = new Vector2(20f, 2f);

        var sprB = new GameObject("SpriteB");
        sprB.transform.SetParent(go.transform);
        sprB.transform.localPosition = new Vector3(20f, 0, 0);
        var srB = sprB.AddComponent<SpriteRenderer>();
        srB.sprite = surfaceSprite;
        srB.sortingLayerName = "Ground";
        srB.sortingOrder = 0;
        srB.drawMode = SpriteDrawMode.Tiled;
        srB.size = new Vector2(20f, 2f);

        // Underground fill
        // AI Art Prompt: Pixel art underground fill. Solid dark brown dirt. Tileable. 64×64px.
        var subSprite = LoadOrCreateSprite("Ground/ground-sub", new Color(0.35f, 0.2f, 0.1f), 64, 64, pixelsPerUnit: 64);

        var subA = new GameObject("SubA");
        subA.transform.SetParent(go.transform);
        subA.transform.localPosition = new Vector3(0f, -1.5f, 0f);
        var subSrA = subA.AddComponent<SpriteRenderer>();
        subSrA.sprite = subSprite;
        subSrA.sortingLayerName = "Ground";
        subSrA.sortingOrder = -1;
        subSrA.drawMode = SpriteDrawMode.Tiled;
        subSrA.size = new Vector2(20f, 3f);

        var subB = new GameObject("SubB");
        subB.transform.SetParent(go.transform);
        subB.transform.localPosition = new Vector3(20f, -1.5f, 0f);
        var subSrB = subB.AddComponent<SpriteRenderer>();
        subSrB.sprite = subSprite;
        subSrB.sortingLayerName = "Ground";
        subSrB.sortingOrder = -1;
        subSrB.drawMode = SpriteDrawMode.Tiled;
        subSrB.size = new Vector2(20f, 3f);

        var so = new SerializedObject(layer);
        so.FindProperty("_scrollSpeedMultiplier").floatValue = 1f;
        so.FindProperty("_spriteA").objectReferenceValue = srA;
        so.FindProperty("_spriteB").objectReferenceValue = srB;
        so.FindProperty("_baseSpeed").floatValue = 3f;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created modular ground layer (surface + underground fill)");
    }

    private static void CreateCloudSpawner()
    {
        if (Object.FindFirstObjectByType<CloudSpawner>() != null) return;

        // Cloud sprites
        // AI Art Prompt: Pixel art small fluffy cloud, white/light grey. Transparent background. 32×16px.
        var cloudSmall = LoadOrCreateSprite("Background/cloud-small", new Color(1f, 1f, 1f, 0.7f), 32, 16, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.Ellipse);
        // AI Art Prompt: Pixel art large fluffy cloud, white/light grey. Transparent background. 64×24px.
        var cloudLarge = LoadOrCreateSprite("Background/cloud-large", new Color(0.95f, 0.95f, 1f, 0.8f), 64, 24, pixelsPerUnit: 64, alphaIsTransparency: true, shape: SpriteShape.Ellipse);

        var go = new GameObject("CloudSpawner");
        var spawner = go.AddComponent<CloudSpawner>();

        var so = new SerializedObject(spawner);
        var spritesProp = so.FindProperty("_cloudSprites");
        spritesProp.arraySize = 2;
        spritesProp.GetArrayElementAtIndex(0).objectReferenceValue = cloudSmall;
        spritesProp.GetArrayElementAtIndex(1).objectReferenceValue = cloudLarge;
        so.FindProperty("_maxClouds").intValue = 6;
        so.FindProperty("_minY").floatValue = -2f;
        so.FindProperty("_maxY").floatValue = 4f;
        so.FindProperty("_minDriftSpeed").floatValue = 0.1f;
        so.FindProperty("_maxDriftSpeed").floatValue = 0.4f;
        so.FindProperty("_spawnInterval").floatValue = 3f;
        so.FindProperty("_minScale").floatValue = 0.5f;
        so.FindProperty("_maxScale").floatValue = 1.2f;
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created CloudSpawner with 2 cloud sprites");
    }

    private static void SetupDifficultyVisualFeedback(Camera cam)
    {
        if (Object.FindFirstObjectByType<DifficultyVisualFeedback>() != null) return;
        if (cam == null) return;

        var go = new GameObject("DifficultyVisualFeedback");
        var feedback = go.AddComponent<DifficultyVisualFeedback>();

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.53f, 0.81f, 0.92f), 0f),
                new GradientColorKey(new Color(0.95f, 0.65f, 0.25f), 0.5f),
                new GradientColorKey(new Color(0.45f, 0.15f, 0.35f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        var speedLineGO = new GameObject("SpeedLines");
        speedLineGO.transform.SetParent(cam.transform);
        speedLineGO.transform.localPosition = new Vector3(2f, 0f, 5f);
        var speedLinePS = speedLineGO.AddComponent<ParticleSystem>();

        var main = speedLinePS.main;
        main.duration = 1f;
        main.startLifetime = 0.3f;
        main.startSpeed = 15f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = new Color(1f, 1f, 1f, 0.4f);
        main.maxParticles = 50;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = speedLinePS.emission;
        emission.rateOverTime = 30f;

        var shape = speedLinePS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.1f, 10f, 1f);
        shape.rotation = new Vector3(0f, 0f, 90f);

        var renderer = speedLineGO.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "UI";
        renderer.sortingOrder = -1;

        var renderers = new System.Collections.Generic.List<SpriteRenderer>();
        foreach (var layerName in new[] { "SkyGradient", "HillsSilhouette", "TreesMidground", "BackgroundNear", "Ground Layer" })
        {
            var layerGO = GameObject.Find(layerName);
            if (layerGO != null)
                renderers.AddRange(layerGO.GetComponentsInChildren<SpriteRenderer>());
        }

        var so = new SerializedObject(feedback);
        so.FindProperty("_mainCamera").objectReferenceValue = cam;
        so.FindProperty("_speedLines").objectReferenceValue = speedLinePS;
        so.FindProperty("_speedLineThreshold").floatValue = 5f;
        so.FindProperty("_maxDifficultyScore").floatValue = 200f;
        so.ApplyModifiedPropertiesWithoutUndo();

        var arrayProp = so.FindProperty("_parallaxRenderers");
        arrayProp.arraySize = renderers.Count;
        for (int i = 0; i < renderers.Count; i++)
            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        var field = typeof(DifficultyVisualFeedback).GetField("_skyGradient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(feedback, gradient);

        EditorUtility.SetDirty(feedback);
        Debug.Log("[GameSetup] Created DifficultyVisualFeedback with sky gradient and speed lines");
    }

    private static void CreateBoundary(string name, Vector3 position)
    {
        if (GameObject.Find(name) != null) return;

        var go = new GameObject(name);
        go.transform.position = position;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(40f, 1f);
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

        // Safe Area
        var safeAreaGO = CreatePanel(canvasGO.transform, "SafeArea");
        safeAreaGO.AddComponent<SafeAreaHandler>();

        // ── Main Menu Panel ──
        var menuPanel = CreatePanel(safeAreaGO.transform, "MainMenuPanel");
        var titleText = CreateTMPText(menuPanel.transform, "TitleText", "FlappyKookaburra", 72);
        SetRectTransform(titleText, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(800, 100));
        var startBtn = CreateButton(menuPanel.transform, "StartButton", "TAP TO START");
        SetRectTransform(startBtn, new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(400, 80));

        // ── HUD Panel ──
        var hudPanel = CreatePanel(safeAreaGO.transform, "HUDPanel");
        hudPanel.SetActive(false);
        var scoreText = CreateTMPText(hudPanel.transform, "ScoreText", "0", 96);
        SetRectTransform(scoreText, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(400, 120));
        var punchEffect = scoreText.AddComponent<ScorePunchEffect>();

        // ── Game Over Panel ──
        var gameOverPanel = CreatePanel(safeAreaGO.transform, "GameOverPanel");
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

        // Medal display placeholder
        var medalGO = new GameObject("MedalDisplay", typeof(RectTransform));
        medalGO.transform.SetParent(gameOverPanel.transform, false);
        var medalImage = medalGO.AddComponent<Image>();
        medalImage.color = new Color(1f, 1f, 1f, 0f); // Hidden by default
        SetRectTransform(medalGO, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(120, 120));

        var restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "RESTART");
        SetRectTransform(restartBtn, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(400, 80));

        // ── Screen Flash Overlay (for death effect) ──
        var flashGO = new GameObject("ScreenFlash", typeof(RectTransform));
        flashGO.transform.SetParent(canvasGO.transform, false);
        var flashImage = flashGO.AddComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashImage.raycastTarget = false;
        var flashRT = flashGO.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;

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
        uiSo.FindProperty("_screenFlash").objectReferenceValue = flashImage;
        uiSo.FindProperty("_medalImage").objectReferenceValue = medalImage;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("[GameSetup] Created UI hierarchy with screen flash and medal display");
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
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.3f, 0.69f, 0.31f, 0.9f); // Material green
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

    // ─── Difficulty Curves ─────────────────────────────────────────

    private static AnimationCurve CreateSCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f,   0f,   0f,    0f),
            new Keyframe(15f,  0.05f, 0.003f, 0.015f),
            new Keyframe(50f,  0.5f,  0.012f, 0.012f),
            new Keyframe(80f,  0.8f,  0.008f, 0.004f),
            new Keyframe(120f, 0.92f, 0.002f, 0.001f),
            new Keyframe(200f, 1f,    0.001f, 0f)
        );
    }

    private static AnimationCurve CreateOscillationCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f,   0f,  0f,    0f),
            new Keyframe(60f,  0f,  0f,    0.005f),
            new Keyframe(100f, 0.5f, 0.012f, 0.012f),
            new Keyframe(150f, 0.9f, 0.005f, 0.002f),
            new Keyframe(200f, 1f,   0.001f, 0f)
        );
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
