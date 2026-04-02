using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;

/// <summary>
/// Automated scene builder for the "Neon Cyberpunk Museum" Product Showcase.
/// Menu: Tools > Build Product Showcase Scene
///
/// This script creates the ENTIRE scene required by Lab 7:
///   - Ground plane + 5 plinths
///   - 6 showcase objects with distinct materials
///   - Directional light + 2 additional lights + three-point lighting
///   - Area light, shadows, static flags
///   - Light Probe Group + Reflection Probe
///   - Organized hierarchy
///   - Procedural textures (checker base map + normal map)
///   - Emissive material
///   - Transparent material
///   - Matte, shiny non-metal, and metal materials
///
/// After running:
///   1. Assign the CyberpunkShimmer Shader Graph material manually to one object
///   2. Bake lighting (Window > Rendering > Lighting > Generate Lighting)
///   3. Save scene to Assets/Scenes/
/// </summary>
public class SceneSetupEditor : Editor
{
    [MenuItem("Tools/Build Product Showcase Scene")]
    static void BuildScene()
    {
        // Clean existing scene objects (optional - comment out if you want to keep existing)
        // ClearScene();

        // ============================================================
        // HIERARCHY PARENTS
        // ============================================================
        GameObject environmentParent = new GameObject("Environment");
        GameObject plinthsParent = new GameObject("Plinths");
        GameObject productsParent = new GameObject("Products");
        GameObject lightsParent = new GameObject("Lights");
        GameObject probesParent = new GameObject("Probes");

        // ============================================================
        // FOLDER SETUP
        // ============================================================
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Textures"))
            AssetDatabase.CreateFolder("Assets", "Textures");

        // ============================================================
        // GENERATE PROCEDURAL TEXTURES
        // ============================================================
        Texture2D checkerTex = GenerateCheckerTexture(256, 256, 16,
            new Color(0.1f, 0.1f, 0.15f), new Color(0.05f, 0.05f, 0.08f));
        SaveTexture(checkerTex, "Assets/Textures/CyberpunkChecker.png");

        Texture2D normalTex = GenerateNormalMap(256, 256);
        SaveTexture(normalTex, "Assets/Textures/CyberpunkNormal.png");

        // Import and set texture types
        AssetDatabase.Refresh();
        SetTextureImportSettings("Assets/Textures/CyberpunkNormal.png", true);

        Texture2D loadedChecker = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/CyberpunkChecker.png");
        Texture2D loadedNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/CyberpunkNormal.png");

        // ============================================================
        // MATERIALS (5+ distinct, all URP/Lit)
        // ============================================================

        // 1. Matte surface (Metallic=0, Smoothness<0.2) -- dark rough stone for plinths
        Material matteMat = CreateURPLitMaterial("M_DarkStone_Matte",
            new Color(0.08f, 0.08f, 0.12f), 0f, 0.1f);

        // 2. Shiny non-metal (Metallic=0, Smoothness>0.7) -- polished ceramic
        Material shinyMat = CreateURPLitMaterial("M_CyanCeramic_Shiny",
            new Color(0.1f, 0.6f, 0.7f), 0f, 0.85f);

        // 3. Metal surface (Metallic=1, Smoothness>0.8) -- chrome
        Material metalMat = CreateURPLitMaterial("M_Chrome_Metal",
            new Color(0.85f, 0.85f, 0.9f), 1f, 0.92f);

        // 4. Transparent surface -- neon glass
        Material transparentMat = CreateURPLitTransparent("M_NeonGlass_Transparent",
            new Color(0.2f, 0.0f, 0.8f, 0.45f));

        // 5. Emissive material -- neon glow
        Material emissiveMat = CreateURPLitEmissive("M_NeonGlow_Emissive",
            new Color(0.02f, 0.02f, 0.05f), new Color(1f, 0.1f, 0.6f), 3f);

        // 6. Textured material with normal map -- cyberpunk floor
        Material texturedMat = CreateURPLitTextured("M_CyberpunkFloor_Textured",
            loadedChecker, loadedNormal, 0f, 0.3f);

        // 7. Gold metal for variety
        Material goldMat = CreateURPLitMaterial("M_Gold_Metal",
            new Color(1f, 0.76f, 0.2f), 1f, 0.85f);

        // ============================================================
        // GROUND PLANE
        // ============================================================
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -0.25f, 0);
        ground.transform.localScale = new Vector3(20, 0.5f, 20);
        ground.GetComponent<Renderer>().sharedMaterial = texturedMat;
        ground.isStatic = true;
        ground.transform.SetParent(environmentParent.transform);

        // Back wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.position = new Vector3(0, 3, 8);
        backWall.transform.localScale = new Vector3(20, 7, 0.3f);
        backWall.GetComponent<Renderer>().sharedMaterial = matteMat;
        backWall.isStatic = true;
        backWall.transform.SetParent(environmentParent.transform);

        // ============================================================
        // PLINTHS (5 platforms)
        // ============================================================
        float[] plinthX = { -4f, -2f, 0f, 2f, 4f };
        float plinthHeight = 1f;
        GameObject[] plinths = new GameObject[5];

        for (int i = 0; i < 5; i++)
        {
            GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "Plinth_" + (i + 1);
            plinth.transform.position = new Vector3(plinthX[i], plinthHeight / 2f, 0);
            plinth.transform.localScale = new Vector3(1.2f, plinthHeight, 1.2f);
            plinth.GetComponent<Renderer>().sharedMaterial = matteMat;
            plinth.isStatic = true;
            plinth.transform.SetParent(plinthsParent.transform);
            plinths[i] = plinth;
        }

        // ============================================================
        // PRODUCT OBJECTS (6 objects on plinths + extra)
        // ============================================================

        // Product 1: Chrome Sphere (metal)
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Product_ChromeSphere";
        sphere.transform.position = new Vector3(-4f, plinthHeight + 0.5f, 0);
        sphere.GetComponent<Renderer>().sharedMaterial = metalMat;
        sphere.transform.SetParent(productsParent.transform);

        // Product 2: Cyan Ceramic Cube (shiny non-metal)
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Product_CyanCube";
        cube.transform.position = new Vector3(-2f, plinthHeight + 0.5f, 0);
        cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        cube.transform.rotation = Quaternion.Euler(0, 45, 0);
        cube.GetComponent<Renderer>().sharedMaterial = shinyMat;
        cube.transform.SetParent(productsParent.transform);

        // Product 3: Neon Glass Capsule (transparent) -- this gets three-point lighting
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Product_NeonCapsule_ThreePointLit";
        capsule.transform.position = new Vector3(0f, plinthHeight + 0.75f, 0);
        capsule.GetComponent<Renderer>().sharedMaterial = transparentMat;
        capsule.transform.SetParent(productsParent.transform);

        // Product 4: Glowing Cylinder (emissive)
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "Product_GlowingCylinder";
        cylinder.transform.position = new Vector3(2f, plinthHeight + 0.5f, 0);
        cylinder.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
        cylinder.GetComponent<Renderer>().sharedMaterial = emissiveMat;
        cylinder.transform.SetParent(productsParent.transform);

        // Product 5: Gold Sphere (metal variant)
        GameObject goldSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goldSphere.name = "Product_GoldSphere";
        goldSphere.transform.position = new Vector3(4f, plinthHeight + 0.5f, 0);
        goldSphere.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        goldSphere.GetComponent<Renderer>().sharedMaterial = goldMat;
        goldSphere.transform.SetParent(productsParent.transform);

        // Product 6: Floating Diamond shape (two pyramids) using rotated cube -- ShaderGraph target
        GameObject diamond = GameObject.CreatePrimitive(PrimitiveType.Cube);
        diamond.name = "Product_ShaderGraphDiamond";
        diamond.transform.position = new Vector3(0f, 3.5f, -2f);
        diamond.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        diamond.transform.rotation = Quaternion.Euler(45, 45, 0);
        // Use emissive for now -- user should replace with ShaderGraph material
        diamond.GetComponent<Renderer>().sharedMaterial = emissiveMat;
        diamond.transform.SetParent(productsParent.transform);

        // ============================================================
        // LIGHTING
        // ============================================================

        // D1. Directional Light -- moonlight blue, angled for night mood
        Light[] existingLights = Object.FindObjectsOfType<Light>();
        foreach (var l in existingLights)
        {
            if (l.type == LightType.Directional)
                DestroyImmediate(l.gameObject);
        }

        GameObject dirLight = new GameObject("DirectionalLight_Moon");
        Light dirLightComp = dirLight.AddComponent<Light>();
        dirLightComp.type = LightType.Directional;
        dirLightComp.color = new Color(0.4f, 0.45f, 0.7f);
        dirLightComp.intensity = 0.6f;
        dirLight.transform.rotation = Quaternion.Euler(45, -30, 0);
        dirLightComp.shadows = LightShadows.Soft;
        dirLight.transform.SetParent(lightsParent.transform);

        // D2. Point Light 1 -- magenta accent
        GameObject pointLight1 = new GameObject("PointLight_Magenta");
        Light pl1 = pointLight1.AddComponent<Light>();
        pl1.type = LightType.Point;
        pl1.color = new Color(1f, 0.1f, 0.5f);
        pl1.intensity = 2.5f;
        pl1.range = 8f;
        pointLight1.transform.position = new Vector3(-3f, 3f, -2f);
        pointLight1.transform.SetParent(lightsParent.transform);

        // D2. Spot Light 1 -- cyan accent
        GameObject spotLight1 = new GameObject("SpotLight_Cyan");
        Light sl1 = spotLight1.AddComponent<Light>();
        sl1.type = LightType.Spot;
        sl1.color = new Color(0f, 0.8f, 1f);
        sl1.intensity = 3f;
        sl1.range = 12f;
        sl1.spotAngle = 45f;
        spotLight1.transform.position = new Vector3(3f, 4f, -3f);
        spotLight1.transform.rotation = Quaternion.Euler(40, -10, 0);
        spotLight1.transform.SetParent(lightsParent.transform);

        // D3. THREE-POINT LIGHTING on the center capsule
        // Key light
        GameObject keyLight = new GameObject("ThreePoint_Key");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Spot;
        kl.color = new Color(1f, 0.95f, 0.8f);
        kl.intensity = 4f;
        kl.range = 8f;
        kl.spotAngle = 35f;
        keyLight.transform.position = new Vector3(-1.5f, 3f, -2.5f);
        keyLight.transform.LookAt(capsule.transform.position);
        keyLight.transform.SetParent(lightsParent.transform);

        // Fill light
        GameObject fillLight = new GameObject("ThreePoint_Fill");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Spot;
        fl.color = new Color(0.5f, 0.5f, 0.8f);
        fl.intensity = 1.5f;
        fl.range = 8f;
        fl.spotAngle = 50f;
        fillLight.transform.position = new Vector3(2f, 2f, -2f);
        fillLight.transform.LookAt(capsule.transform.position);
        fillLight.transform.SetParent(lightsParent.transform);

        // Rim / back light
        GameObject rimLight = new GameObject("ThreePoint_Rim");
        Light rl = rimLight.AddComponent<Light>();
        rl.type = LightType.Spot;
        rl.color = new Color(0.8f, 0.2f, 1f);
        rl.intensity = 3f;
        rl.range = 6f;
        rl.spotAngle = 40f;
        rimLight.transform.position = new Vector3(0, 2.5f, 2.5f);
        rimLight.transform.LookAt(capsule.transform.position);
        rimLight.transform.SetParent(lightsParent.transform);

        // E4. Area Light (baked) -- soft illumination from above
        GameObject areaLight = new GameObject("AreaLight_Ceiling");
        Light al = areaLight.AddComponent<Light>();
        al.type = LightType.Rectangle;
        al.color = new Color(0.6f, 0.5f, 0.8f);
        al.intensity = 4f;
        al.areaSize = new Vector2(8f, 4f);
#if UNITY_EDITOR
        al.lightmapBakeType = LightmapBakeType.Baked;
#endif
        areaLight.transform.position = new Vector3(0, 5, 0);
        areaLight.transform.rotation = Quaternion.Euler(90, 0, 0);
        areaLight.transform.SetParent(lightsParent.transform);

        // Set some lights to Baked/Mixed for lightmap requirement
        pl1.lightmapBakeType = LightmapBakeType.Mixed;
        sl1.lightmapBakeType = LightmapBakeType.Mixed;

        // D4. Ambient lighting -- dark gradient for cyberpunk mood
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.02f, 0.01f, 0.05f);
        RenderSettings.ambientEquatorColor = new Color(0.05f, 0.02f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.01f, 0.01f, 0.02f);

        // ============================================================
        // PROBES
        // ============================================================

        // F1. Light Probe Group
        GameObject lightProbeObj = new GameObject("LightProbeGroup");
        LightProbeGroup lpg = lightProbeObj.AddComponent<LightProbeGroup>();
        // Place probes in a grid where lighting changes
        Vector3[] probePositions = new Vector3[]
        {
            // Lower row
            new Vector3(-4, 0.5f, -1), new Vector3(-2, 0.5f, -1), new Vector3(0, 0.5f, -1),
            new Vector3(2, 0.5f, -1), new Vector3(4, 0.5f, -1),
            // Mid row
            new Vector3(-4, 2f, -1), new Vector3(-2, 2f, -1), new Vector3(0, 2f, -1),
            new Vector3(2, 2f, -1), new Vector3(4, 2f, -1),
            // Upper row
            new Vector3(-4, 3.5f, -1), new Vector3(-2, 3.5f, -1), new Vector3(0, 3.5f, -1),
            new Vector3(2, 3.5f, -1), new Vector3(4, 3.5f, -1),
            // Behind objects
            new Vector3(-4, 1.5f, 1), new Vector3(0, 1.5f, 1), new Vector3(4, 1.5f, 1),
        };
        lpg.probePositions = probePositions;
        lightProbeObj.transform.SetParent(probesParent.transform);

        // F2. Reflection Probe (near chrome sphere for visible reflections)
        GameObject reflProbeObj = new GameObject("ReflectionProbe_Chrome");
        ReflectionProbe rp = reflProbeObj.AddComponent<ReflectionProbe>();
        rp.mode = ReflectionProbeMode.Baked;
        rp.boxProjection = true;
        rp.size = new Vector3(4, 4, 4);
        reflProbeObj.transform.position = new Vector3(-4f, 2f, 0);
        reflProbeObj.transform.SetParent(probesParent.transform);

        // ============================================================
        // MARK ALL ENVIRONMENT AS STATIC (E2)
        // ============================================================
        SetStaticRecursive(environmentParent);
        SetStaticRecursive(plinthsParent);

        // ============================================================
        // SAVE SCENE
        // ============================================================
        Debug.Log("=== Product Showcase Scene Built Successfully! ===");
        Debug.Log("Theme: Neon Cyberpunk Museum");
        Debug.Log("");
        Debug.Log("NEXT STEPS:");
        Debug.Log("1. Assign your CyberpunkShimmer Shader Graph material to 'Product_ShaderGraphDiamond'");
        Debug.Log("2. Save the scene to Assets/Scenes/ProductShowcase.unity");
        Debug.Log("3. Window > Rendering > Lighting > Generate Lighting (to bake lightmaps)");
        Debug.Log("4. Check off all items on your worksheet!");

        // Select the root objects to make them visible in hierarchy
        Selection.activeGameObject = productsParent;
        EditorUtility.DisplayDialog("Scene Built!",
            "Neon Cyberpunk Museum scene created!\n\n" +
            "Next steps:\n" +
            "1. Assign ShaderGraph material to 'Product_ShaderGraphDiamond'\n" +
            "2. Save scene to Assets/Scenes/\n" +
            "3. Bake lighting (Window > Rendering > Lighting)\n" +
            "4. Check your worksheet!",
            "OK");
    }

    // ================================================================
    // HELPER METHODS
    // ================================================================

    static Material CreateURPLitMaterial(string name, Color baseColor, float metallic, float smoothness)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("URP Lit shader not found! Make sure you're using a URP project.");
            urpLit = Shader.Find("Standard");
        }

        Material mat = new Material(urpLit);
        mat.name = name;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);

        AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat");
        return mat;
    }

    static Material CreateURPLitTransparent(string name, Color baseColor)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard");

        Material mat = new Material(urpLit);
        mat.name = name;

        // Set surface type to Transparent
        mat.SetFloat("_Surface", 1); // 0=Opaque, 1=Transparent
        mat.SetFloat("_Blend", 0);   // 0=Alpha
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.9f);

        AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat");
        return mat;
    }

    static Material CreateURPLitEmissive(string name, Color baseColor, Color emissionColor, float emissionIntensity)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard");

        Material mat = new Material(urpLit);
        mat.name = name;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.5f);

        // Enable emission
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

        AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat");
        return mat;
    }

    static Material CreateURPLitTextured(string name, Texture2D baseMap, Texture2D normalMap, float metallic, float smoothness)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Standard");

        Material mat = new Material(urpLit);
        mat.name = name;

        if (baseMap != null)
            mat.SetTexture("_BaseMap", baseMap);
        if (normalMap != null)
        {
            mat.SetTexture("_BumpMap", normalMap);
            mat.SetFloat("_BumpScale", 1.5f);
            mat.EnableKeyword("_NORMALMAP");
        }

        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetTextureScale("_BaseMap", new Vector2(4, 4));
        mat.SetTextureScale("_BumpMap", new Vector2(4, 4));

        AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat");
        return mat;
    }

    static Texture2D GenerateCheckerTexture(int width, int height, int tileSize, Color color1, Color color2)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isEven = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                tex.SetPixel(x, y, isEven ? color1 : color2);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D GenerateNormalMap(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a subtle grid/groove pattern
                float nx = 0.5f;
                float ny = 0.5f;

                // Horizontal grooves
                if (y % 32 < 2) ny = 0.3f;
                if (y % 32 > 30) ny = 0.7f;
                // Vertical grooves
                if (x % 32 < 2) nx = 0.3f;
                if (x % 32 > 30) nx = 0.7f;

                tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
            }
        }
        tex.Apply();
        return tex;
    }

    static void SaveTexture(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath, "..", path);
        File.WriteAllBytes(fullPath, bytes);
    }

    static void SetTextureImportSettings(string path, bool isNormalMap)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (isNormalMap)
                importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();
        }
    }

    static void SetStaticRecursive(GameObject go)
    {
        go.isStatic = true;
        foreach (Transform child in go.transform)
            SetStaticRecursive(child.gameObject);
    }
}
