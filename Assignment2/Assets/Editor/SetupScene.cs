using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;

public class SetupScene : Editor
{
    private const string SPRITE_SHEET_PATH = "Assets/Roguelike2D/TutorialAssets/Sprites/SandTheme.png";
    private const string AUDIO_PATH = "Assets/Roguelike2D/TutorialAssets/Audio/";
    private const string PREFAB_PATH = "Assets/Prefabs/";
    private const string ANIM_PATH = "Assets/Animations/";
    private const string VFX_PATH = "Assets/VFX/";
    private const string UI_PATH = "Assets/UI/GameUI.uxml";

    // Sprite sheet layout (verified via pixel analysis of SandTheme.png):
    // Sprite indices confirmed by user inspection of SandTheme.png sub-sprites
    private const int PLAYER_IDLE   = 1;
    private static readonly int[] PLAYER_WALK_FRAMES   = { 5, 6, 7, 8 };
    private static readonly int[] PLAYER_ATTACK_FRAMES = { 10, 11, 12 };

    private const int ENEMY_IDLE    = 20;
    private static readonly int[] ENEMY_WALK_FRAMES    = { 13, 14, 15, 16 };
    private static readonly int[] ENEMY_ATTACK_FRAMES  = { 21, 22, 23 };

    private const int EXIT_SPRITE   = 28;
    private const int FOOD1_SPRITE  = 30;
    private const int FOOD2_SPRITE  = 26;
    private const int WALL_SPRITE   = 41;
    private const int WALL_DAMAGED  = 42;

    [MenuItem("Tools/Roguelike/Reimport Sprites")]
    public static void ReimportSprites()
    {
        // Force reimport all sprite sheets so they are properly sliced
        string[] spritePaths = {
            "Assets/Roguelike2D/TutorialAssets/Sprites/SandTheme.png",
            "Assets/Roguelike2D/TutorialAssets/Sprites/SnowTheme.png",
            "Assets/Roguelike2D/TutorialAssets/Sprites/SnowTheme2.png",
            "Assets/Roguelike2D/TutorialAssets/Sprites/UrbanTheme.png",
            "Assets/Roguelike2D/TutorialAssets/Sprites/UrbanTheme2.png"
        };
        foreach (string path in spritePaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();
        Debug.Log("Sprites reimported. Now run Tools > Roguelike > Setup Scene");
    }

    [MenuItem("Tools/Roguelike/Clean Scene")]
    public static void CleanScene()
    {
        // Destroy all non-camera scene objects
        string[] toDestroy = { "Grid", "GameManager", "BoardManager", "PlayerCharacter",
            "AudioManager", "VFXManager", "ObjectPool", "UIDocument", "Directional Light" };
        foreach (string name in toDestroy)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) DestroyImmediate(go);
        }

        // Delete generated asset folders
        string[] foldersToDelete = { "Assets/Tiles", "Assets/Prefabs", "Assets/Animations", "Assets/VFX" };
        foreach (string folder in foldersToDelete)
        {
            if (AssetDatabase.IsValidFolder(folder))
                AssetDatabase.DeleteAsset(folder);
        }

        AssetDatabase.Refresh();
        Debug.Log("Scene cleaned. Ready for fresh setup.");
    }

    [MenuItem("Tools/Roguelike/Setup Scene")]
    public static void SetupFullScene()
    {
        // Clean first
        CleanScene();

        EnsureDirectories();

        // Force reimport sprite sheet first
        AssetDatabase.ImportAsset(SPRITE_SHEET_PATH, ImportAssetOptions.ForceUpdate);

        // Load sprite sheet
        Sprite[] sprites = LoadSpriteSheet();
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("Could not load sprite sheet at: " + SPRITE_SHEET_PATH);
            return;
        }

        Debug.Log($"Loaded {sprites.Length} sprites. Names:");
        for (int i = 0; i < sprites.Length; i++)
            Debug.Log($"  [{i}] {sprites[i].name} ({sprites[i].rect})");

        // Create animator controllers FIRST so we can assign them into prefabs
        var playerController = CreatePlayerAnimator(sprites);
        var enemyController = CreateEnemyAnimator(sprites);

        // Create prefabs WITH controllers already assigned
        GameObject playerPrefab = CreatePlayerPrefab(sprites, playerController);
        GameObject[] foodPrefabs = CreateFoodPrefabs(sprites);
        GameObject wallPrefab = CreateWallPrefab(sprites);
        GameObject enemyPrefab = CreateEnemyPrefab(sprites, enemyController);
        GameObject exitPrefab = CreateExitPrefab(sprites);

        // Create VFX prefabs
        GameObject wallVFX = CreateParticleEffect("WallDestructionVFX", new Color(0.6f, 0.4f, 0.2f));
        GameObject foodVFX = CreateParticleEffect("FoodCollectVFX", new Color(0.2f, 1f, 0.2f));
        GameObject enemyVFX = CreateParticleEffect("EnemyDeathVFX", new Color(1f, 0.2f, 0.2f));

        // Setup the scene
        SetupSceneHierarchy(playerPrefab, foodPrefabs, wallPrefab, enemyPrefab, exitPrefab,
                           wallVFX, foodVFX, enemyVFX, sprites);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Scene setup complete! Press Play to test.");
    }

    private static void EnsureDirectories()
    {
        string[] dirs = { PREFAB_PATH, ANIM_PATH, VFX_PATH };
        foreach (string dir in dirs)
        {
            if (!AssetDatabase.IsValidFolder(dir.TrimEnd('/')))
            {
                string parent = Path.GetDirectoryName(dir.TrimEnd('/'));
                string folder = Path.GetFileName(dir.TrimEnd('/'));
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }

    private static Sprite[] LoadSpriteSheet()
    {
        Object[] loaded = AssetDatabase.LoadAllAssetsAtPath(SPRITE_SHEET_PATH);
        // Filter to only Scavengers2_SpriteSheet sprites and sort numerically
        return loaded.OfType<Sprite>()
            .Where(s => s.name.StartsWith("Scavengers2_SpriteSheet_"))
            .OrderBy(s => {
                string numStr = s.name.Replace("Scavengers2_SpriteSheet_", "");
                int.TryParse(numStr, out int num);
                return num;
            })
            .ToArray();
    }

    private static Sprite FindSpriteByIndex(Sprite[] sprites, int index)
    {
        if (index >= 0 && index < sprites.Length)
            return sprites[index];
        return null;
    }

    // ---- PREFAB CREATION ----

    private static GameObject CreatePlayerPrefab(Sprite[] sprites, UnityEditor.Animations.AnimatorController controller)
    {
        GameObject go = new GameObject("PlayerCharacter");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FindSpriteByIndex(sprites, PLAYER_IDLE);
        sr.sortingOrder = 2;

        go.AddComponent<PlayerController>();

        Animator anim = go.AddComponent<Animator>();
        if (controller != null)
            anim.runtimeAnimatorController = controller;

        string path = PREFAB_PATH + "PlayerCharacter.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        Debug.Log("Created player prefab with animator controller");
        return prefab;
    }

    private static GameObject[] CreateFoodPrefabs(Sprite[] sprites)
    {
        GameObject[] prefabs = new GameObject[2];

        // Food 1 - fruit (lower food value)
        prefabs[0] = CreateFoodPrefab("FoodFruit", FindSpriteByIndex(sprites, FOOD1_SPRITE), 10);
        // Food 2 - soda (higher food value)
        prefabs[1] = CreateFoodPrefab("FoodSoda", FindSpriteByIndex(sprites, FOOD2_SPRITE), 20);

        return prefabs;
    }

    private static GameObject CreateFoodPrefab(string name, Sprite sprite, int amount)
    {
        GameObject go = new GameObject(name);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        FoodObject food = go.AddComponent<FoodObject>();
        // Set AmountGranted via SerializedObject
        string path = PREFAB_PATH + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);

        // Set the private amountGranted field
        SerializedObject so = new SerializedObject(prefab.GetComponent<FoodObject>());
        SerializedProperty amountProp = so.FindProperty("amountGranted");
        if (amountProp != null)
        {
            amountProp.intValue = amount;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        DestroyImmediate(go);
        Debug.Log($"Created food prefab: {name} (grants {amount})");
        return prefab;
    }

    private static GameObject CreateWallPrefab(Sprite[] sprites)
    {
        GameObject go = new GameObject("Wall");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FindSpriteByIndex(sprites, WALL_SPRITE);
        sr.sortingOrder = 1;

        WallObject wall = go.AddComponent<WallObject>();

        string path = PREFAB_PATH + "Wall.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);

        // Set the damaged sprite
        SerializedObject so = new SerializedObject(prefab.GetComponent<WallObject>());
        SerializedProperty damagedProp = so.FindProperty("damagedSprite");
        if (damagedProp != null)
        {
            damagedProp.objectReferenceValue = FindSpriteByIndex(sprites, WALL_DAMAGED);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        DestroyImmediate(go);
        Debug.Log("Created wall prefab");
        return prefab;
    }

    private static GameObject CreateEnemyPrefab(Sprite[] sprites, UnityEditor.Animations.AnimatorController controller)
    {
        GameObject go = new GameObject("Enemy");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FindSpriteByIndex(sprites, ENEMY_IDLE);
        sr.sortingOrder = 2;

        go.AddComponent<Enemy>();

        Animator anim = go.AddComponent<Animator>();
        if (controller != null)
            anim.runtimeAnimatorController = controller;

        string path = PREFAB_PATH + "Enemy.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        Debug.Log("Created enemy prefab with animator controller");
        return prefab;
    }

    private static GameObject CreateExitPrefab(Sprite[] sprites)
    {
        GameObject go = new GameObject("Exit");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = FindSpriteByIndex(sprites, EXIT_SPRITE);
        sr.sortingOrder = 1;

        go.AddComponent<ExitCellObject>();

        string path = PREFAB_PATH + "Exit.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        Debug.Log("Created exit prefab");
        return prefab;
    }

    // ---- ANIMATOR CONTROLLERS ----

    private static UnityEditor.Animations.AnimatorController CreatePlayerAnimator(Sprite[] sprites)
    {
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(
            ANIM_PATH + "PlayerAnimator.controller");

        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Idle — single frame
        var idleClip = CreateSingleFrameClip("PlayerIdle", FindSpriteByIndex(sprites, PLAYER_IDLE));
        var idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;

        // Walk — 4 frames (indices 5,6,7,8) at 8 fps
        var walkSprites = System.Array.ConvertAll(PLAYER_WALK_FRAMES, i => FindSpriteByIndex(sprites, i));
        var walkClip = CreateMultiFrameClip("PlayerWalk", walkSprites, 8f, true);
        var walkState = rootStateMachine.AddState("Walk");
        walkState.motion = walkClip;

        // Attack — 3 frames (indices 10,11,12) at 12 fps, no loop
        var attackSprites = System.Array.ConvertAll(PLAYER_ATTACK_FRAMES, i => FindSpriteByIndex(sprites, i));
        var attackClip = CreateMultiFrameClip("PlayerAttack", attackSprites, 12f, false);
        var attackState = rootStateMachine.AddState("Attack");
        attackState.motion = attackClip;

        // Transitions
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0;

        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0;

        var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0;

        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0;

        EditorUtility.SetDirty(controller);
        Debug.Log("Created player animator controller");
        return controller;
    }

    private static UnityEditor.Animations.AnimatorController CreateEnemyAnimator(Sprite[] sprites)
    {
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(
            ANIM_PATH + "EnemyAnimator.controller");

        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

        var rootStateMachine = controller.layers[0].stateMachine;

        // Idle — single frame (index 20)
        var idleClip = CreateSingleFrameClip("EnemyIdle", FindSpriteByIndex(sprites, ENEMY_IDLE));
        var idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;

        // Walk/Hit — 4 frames (indices 13-16) at 8 fps — used as "hit" reaction
        var hitSprites = System.Array.ConvertAll(ENEMY_WALK_FRAMES, i => FindSpriteByIndex(sprites, i));
        var hitClip = CreateMultiFrameClip("EnemyHit", hitSprites, 8f, false);
        var hitState = rootStateMachine.AddState("Hit");
        hitState.motion = hitClip;

        // Attack — 3 frames (indices 21-23) at 12 fps, no loop
        var attackSprites = System.Array.ConvertAll(ENEMY_ATTACK_FRAMES, i => FindSpriteByIndex(sprites, i));
        var attackClip = CreateMultiFrameClip("EnemyAttack", attackSprites, 12f, false);
        var attackState = rootStateMachine.AddState("Attack");
        attackState.motion = attackClip;

        // Transitions
        var anyToHit = rootStateMachine.AddAnyStateTransition(hitState);
        anyToHit.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Hit");
        anyToHit.hasExitTime = false;
        anyToHit.duration = 0;

        var hitToIdle = hitState.AddTransition(idleState);
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 1f;
        hitToIdle.duration = 0;

        var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0;

        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 1f;
        attackToIdle.duration = 0;

        EditorUtility.SetDirty(controller);
        Debug.Log("Created enemy animator controller");
        return controller;
    }

    private static AnimationClip CreateSingleFrameClip(string name, Sprite sprite)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = 12;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[1];
        keyframes[0] = new ObjectReferenceKeyframe { time = 0, value = sprite };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AssetDatabase.CreateAsset(clip, ANIM_PATH + name + ".anim");
        return clip;
    }

    /// <summary>
    /// Creates an animation clip with any number of evenly-spaced sprite frames.
    /// </summary>
    private static AnimationClip CreateMultiFrameClip(string name, Sprite[] frameSprites, float fps, bool loop)
    {
        AnimationClip clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = fps;

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        float frameDuration = 1f / fps;
        // Each frame gets a keyframe; add a duplicate at the end so Unity doesn't stretch the last frame
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameSprites.Length + 1];
        for (int i = 0; i < frameSprites.Length; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i * frameDuration, value = frameSprites[i] };
        // End-of-clip sentinel (holds last frame)
        keyframes[frameSprites.Length] = new ObjectReferenceKeyframe
        {
            time = frameSprites.Length * frameDuration,
            value = frameSprites[frameSprites.Length - 1]
        };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AssetDatabase.CreateAsset(clip, ANIM_PATH + name + ".anim");
        return clip;
    }

    // ---- VFX ----

    private static GameObject CreateParticleEffect(string name, Color color)
    {
        GameObject go = new GameObject(name);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = color;
        main.maxParticles = 20;
        main.duration = 0.3f;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 10)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        // Use default particle material
        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        psr.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");

        string path = VFX_PATH + name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        Debug.Log($"Created VFX prefab: {name}");
        return prefab;
    }

    // ---- SCENE SETUP ----

    private static void SetupSceneHierarchy(GameObject playerPrefab, GameObject[] foodPrefabs,
        GameObject wallPrefab, GameObject enemyPrefab, GameObject exitPrefab,
        GameObject wallVFX, GameObject foodVFX, GameObject enemyVFX, Sprite[] sprites)
    {
        // Create Grid with Tilemaps
        GameObject grid = new GameObject("Grid");
        grid.AddComponent<Grid>();

        GameObject groundTilemapGO = new GameObject("GroundTilemap");
        groundTilemapGO.transform.SetParent(grid.transform);
        Tilemap groundTilemap = groundTilemapGO.AddComponent<Tilemap>();
        TilemapRenderer groundRenderer = groundTilemapGO.AddComponent<TilemapRenderer>();
        groundRenderer.sortingOrder = 0;

        GameObject wallTilemapGO = new GameObject("WallTilemap");
        wallTilemapGO.transform.SetParent(grid.transform);
        Tilemap wallTilemap = wallTilemapGO.AddComponent<Tilemap>();
        TilemapRenderer wallRenderer = wallTilemapGO.AddComponent<TilemapRenderer>();
        wallRenderer.sortingOrder = 1;

        // Create solid-color tiles (no sprite guessing needed)
        Color[] groundColors = new Color[] {
            new Color(0.82f, 0.71f, 0.55f), // tan
            new Color(0.76f, 0.65f, 0.50f), // darker tan
            new Color(0.85f, 0.74f, 0.58f), // lighter tan
            new Color(0.80f, 0.69f, 0.52f), // mid tan
        };
        Color[] wallColors = new Color[] {
            new Color(0.28f, 0.24f, 0.20f), // dark brown
            new Color(0.32f, 0.28f, 0.22f), // slightly lighter brown
            new Color(0.25f, 0.22f, 0.18f), // darker brown
        };
        Tile[] groundTiles = CreateSolidColorTiles("Ground", groundColors);
        Tile[] wallTiles = CreateSolidColorTiles("Wall", wallColors);

        // --- GameManager ---
        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();

        // --- BoardManager ---
        GameObject bmGO = new GameObject("BoardManager");
        BoardManager bm = bmGO.AddComponent<BoardManager>();

        // --- Player (scene instance for reference, actual spawning is code-driven) ---
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance.name = "PlayerCharacter";
        PlayerController pc = playerInstance.GetComponent<PlayerController>();

        // --- AudioManager ---
        GameObject amGO = new GameObject("AudioManager");
        AudioManager am = amGO.AddComponent<AudioManager>();
        SetupAudioManager(am);

        // --- VFXManager ---
        GameObject vfxGO = new GameObject("VFXManager");
        VFXManager vfx = vfxGO.AddComponent<VFXManager>();
        SetupVFXManager(vfx, wallVFX, foodVFX, enemyVFX);

        // --- ObjectPool ---
        GameObject poolGO = new GameObject("ObjectPool");
        ObjectPool pool = poolGO.AddComponent<ObjectPool>();

        // --- UIDocument ---
        // Create PanelSettings asset (required for UIDocument to render)
        string panelSettingsPath = "Assets/UI/GamePanelSettings.asset";
        PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/UI"))
                AssetDatabase.CreateFolder("Assets", "UI");
            AssetDatabase.CreateAsset(panelSettings, panelSettingsPath);
            AssetDatabase.SaveAssets();
            panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
        }

        GameObject uiGO = new GameObject("UIDocument");
        UIDocument uidoc = uiGO.AddComponent<UIDocument>();
        VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UI_PATH);

        // Use public API to assign (avoids serialized property name issues)
        uidoc.panelSettings = panelSettings;
        if (uxml != null)
            uidoc.visualTreeAsset = uxml;

        EditorUtility.SetDirty(uidoc);
        Debug.Log($"UIDocument configured: panelSettings={panelSettings != null}, uxml={uxml != null}");

        // --- Wire up GameManager references ---
        SerializedObject gmSO = new SerializedObject(gm);
        SetReference(gmSO, "boardManager", bm);
        SetReference(gmSO, "playerController", pc);
        SetReference(gmSO, "uiDocument", uidoc);
        gmSO.ApplyModifiedPropertiesWithoutUndo();

        // --- Wire up BoardManager references ---
        SerializedObject bmSO = new SerializedObject(bm);
        SetReference(bmSO, "groundTilemap", groundTilemap);
        SetReference(bmSO, "wallTilemap", wallTilemap);

        // Ground tiles array
        SerializedProperty groundTilesProp = bmSO.FindProperty("groundTiles");
        if (groundTilesProp != null)
        {
            groundTilesProp.arraySize = groundTiles.Length;
            for (int i = 0; i < groundTiles.Length; i++)
                groundTilesProp.GetArrayElementAtIndex(i).objectReferenceValue = groundTiles[i];
        }

        // Wall tiles array
        SerializedProperty wallTilesProp = bmSO.FindProperty("wallTiles");
        if (wallTilesProp != null)
        {
            wallTilesProp.arraySize = wallTiles.Length;
            for (int i = 0; i < wallTiles.Length; i++)
                wallTilesProp.GetArrayElementAtIndex(i).objectReferenceValue = wallTiles[i];
        }

        // Food prefabs array
        SerializedProperty foodProp = bmSO.FindProperty("foodPrefabs");
        if (foodProp != null)
        {
            foodProp.arraySize = foodPrefabs.Length;
            for (int i = 0; i < foodPrefabs.Length; i++)
                foodProp.GetArrayElementAtIndex(i).objectReferenceValue = foodPrefabs[i].GetComponent<FoodObject>();
        }

        // Wall prefab
        SetReference(bmSO, "wallPrefab", wallPrefab.GetComponent<WallObject>());

        // Enemy prefab
        SetReference(bmSO, "enemyPrefab", enemyPrefab.GetComponent<Enemy>());

        // Exit prefab
        SetReference(bmSO, "exitPrefab", exitPrefab.GetComponent<ExitCellObject>());

        bmSO.ApplyModifiedPropertiesWithoutUndo();

        // --- Wire up ObjectPool ---
        SerializedObject poolSO = new SerializedObject(pool);
        SerializedProperty entriesProp = poolSO.FindProperty("poolEntries");
        if (entriesProp != null)
        {
            // Pool entries: food1, food2, wall, enemy, exit
            CellObject[] poolPrefabs = new CellObject[] {
                foodPrefabs[0].GetComponent<FoodObject>(),
                foodPrefabs[1].GetComponent<FoodObject>(),
                wallPrefab.GetComponent<WallObject>(),
                enemyPrefab.GetComponent<Enemy>(),
                exitPrefab.GetComponent<ExitCellObject>()
            };

            entriesProp.arraySize = poolPrefabs.Length;
            for (int i = 0; i < poolPrefabs.Length; i++)
            {
                SerializedProperty entry = entriesProp.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("prefab").objectReferenceValue = poolPrefabs[i];
                entry.FindPropertyRelative("poolSize").intValue = 10;
            }
        }
        poolSO.ApplyModifiedPropertiesWithoutUndo();

        // Configure camera to center on board
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(4f, 4f, -10f);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
            mainCam.backgroundColor = Color.black;
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Scene hierarchy setup complete!");
    }

    private static void SetReference(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
            prop.objectReferenceValue = value;
        else
            Debug.LogWarning($"Property '{propertyName}' not found on {so.targetObject.GetType().Name}");
    }

    private static Sprite CreateSolidColorSprite(string name, Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        string texPath = "Assets/Tiles/" + name + ".png";
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

        // Set texture import settings for sprite
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
    }

    private static Tile[] CreateSolidColorTiles(string prefix, Color[] colors)
    {
        string tilePath = "Assets/Tiles/";
        if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        Tile[] tiles = new Tile[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Sprite sprite = CreateSolidColorSprite($"{prefix}_{i}", colors[i]);
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.name = $"{prefix}_{i}";
            string path = tilePath + $"{prefix}_{i}.asset";
            AssetDatabase.CreateAsset(tile, path);
            tiles[i] = tile;
        }
        Debug.Log($"Created {tiles.Length} {prefix} tiles (solid color)");
        return tiles;
    }

    private static void SetupAudioManager(AudioManager am)
    {
        SerializedObject so = new SerializedObject(am);

        // Map audio clips
        SetAudioClip(so, "backgroundMusic", AUDIO_PATH + "music.aif");
        SetAudioClip(so, "playerMoveClip", AUDIO_PATH + "footstep1.aif");
        SetAudioClip(so, "wallAttackClip", AUDIO_PATH + "chop1.aif");
        SetAudioClip(so, "foodPickupClip", AUDIO_PATH + "fruit1.aif");
        SetAudioClip(so, "enemyAttackClip", AUDIO_PATH + "enemy1.aif");
        SetAudioClip(so, "enemyDeathClip", AUDIO_PATH + "enemy2.aif");
        SetAudioClip(so, "gameOverClip", AUDIO_PATH + "down.aif");

        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("AudioManager configured with all clips");
    }

    private static void SetAudioClip(SerializedObject so, string propertyName, string assetPath)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        if (clip != null)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
                prop.objectReferenceValue = clip;
        }
        else
        {
            Debug.LogWarning($"Audio clip not found: {assetPath}");
        }
    }

    private static void SetupVFXManager(VFXManager vfx, GameObject wallVFX, GameObject foodVFX, GameObject enemyVFX)
    {
        SerializedObject so = new SerializedObject(vfx);

        SetReference(so, "wallDestructionPrefab",
            wallVFX != null ? wallVFX.GetComponent<ParticleSystem>() : null);
        SetReference(so, "foodCollectPrefab",
            foodVFX != null ? foodVFX.GetComponent<ParticleSystem>() : null);
        SetReference(so, "enemyDeathPrefab",
            enemyVFX != null ? enemyVFX.GetComponent<ParticleSystem>() : null);

        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("VFXManager configured with particle prefabs");
    }

    // ---- BUILD ----

    [MenuItem("Tools/Roguelike/Build All Platforms")]
    public static void BuildAll()
    {
        string buildRoot = "Builds";

        string[] scenes = new string[] {
            "Assets/Scenes/SampleScene.unity"
        };

        // Windows build
        Debug.Log("Building for Windows...");
        BuildPipeline.BuildPlayer(scenes,
            Path.Combine(buildRoot, "Windows", "Roguelike2D.exe"),
            BuildTarget.StandaloneWindows64, BuildOptions.None);

        // macOS build
        Debug.Log("Building for macOS...");
        BuildPipeline.BuildPlayer(scenes,
            Path.Combine(buildRoot, "macOS", "Roguelike2D.app"),
            BuildTarget.StandaloneOSX, BuildOptions.None);

        // WebGL build
        Debug.Log("Building for WebGL...");
        BuildPipeline.BuildPlayer(scenes,
            Path.Combine(buildRoot, "WebGL"),
            BuildTarget.WebGL, BuildOptions.None);

        Debug.Log("All builds complete! Check the Builds/ folder.");
    }
}
