#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class TopographicMapGenerator : EditorWindow
{
    [Header("Terrain")]
    [SerializeField] private Terrain selectedTerrain;

    [Header("Map Output")]
    [SerializeField] private int resolution = 1024;
    [SerializeField] private string outputFolder = "Assets/GeneratedMaps";

    [Header("Map GameObject")]
    [Tooltip("Example: Map/Topographic")]
    [SerializeField] private string mapRootPath = "Map/Topographic";

    [SerializeField] private string mapLayerName = "Map";

    [Tooltip("Height above terrain at which the map quads are placed.")]
    [SerializeField] private float quadHeightOffset = 1f;

    [Header("Elevation")]
    [SerializeField] private float contourInterval = 50f;
    [SerializeField] private float waterLevel = 0f;

    [Header("Appearance")]
    [SerializeField] private Color landColor = Color.white;
    [SerializeField] private Color contourColor = Color.black;
    [SerializeField]
    private Color waterColor =
        new Color(0.15f, 0.45f, 0.85f, 1f);

    [SerializeField] private int contourThicknessPixels = 1;
    [SerializeField] private bool outlineWater = true;

    [Header("Build")]
    [SerializeField] private bool replaceExistingTiles = true;
    [SerializeField] private bool buildMapQuadsAutomatically = true;

    [MenuItem("Tools/Topographic Map Generator")]
    private static void OpenWindow()
    {
        GetWindow<TopographicMapGenerator>("Topographic Map");
    }

    [MenuItem("Tools/Fix Terrain Distances")]
    static void FixDistances()
    {
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        foreach (Terrain terrain in terrains)
        {
            terrain.treeMaximumFullLODCount = 0; // Adjust if your original was different
            EditorUtility.SetDirty(terrain);
        }
        Debug.Log($"Updated tree distance for {terrains.Length} terrains.");
    }


private void OnGUI()
    {
        GUILayout.Label(
            "Topographic Map Generator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        selectedTerrain = (Terrain)EditorGUILayout.ObjectField(
            "Terrain",
            selectedTerrain,
            typeof(Terrain),
            true
        );

        EditorGUILayout.Space();

        resolution = EditorGUILayout.IntField(
            "Resolution",
            resolution
        );

        outputFolder = EditorGUILayout.TextField(
            "Output Folder",
            outputFolder
        );

        EditorGUILayout.Space();

        mapRootPath = EditorGUILayout.TextField(
            "Map Root Path",
            mapRootPath
        );

        mapLayerName = EditorGUILayout.TextField(
            "Map Layer",
            mapLayerName
        );

        quadHeightOffset = EditorGUILayout.FloatField(
            "Quad Height Offset",
            quadHeightOffset
        );

        EditorGUILayout.Space();

        contourInterval = EditorGUILayout.FloatField(
            "Contour Interval (m)",
            contourInterval
        );

        waterLevel = EditorGUILayout.FloatField(
            "Water Level",
            waterLevel
        );

        EditorGUILayout.Space();

        landColor = EditorGUILayout.ColorField(
            "Land",
            landColor
        );

        contourColor = EditorGUILayout.ColorField(
            "Contours",
            contourColor
        );

        waterColor = EditorGUILayout.ColorField(
            "Water",
            waterColor
        );

        contourThicknessPixels = EditorGUILayout.IntSlider(
            "Contour Thickness",
            contourThicknessPixels,
            1,
            5
        );

        outlineWater = EditorGUILayout.Toggle(
            "Outline Water",
            outlineWater
        );

        EditorGUILayout.Space();

        replaceExistingTiles = EditorGUILayout.Toggle(
            "Replace Existing Tiles",
            replaceExistingTiles
        );

        buildMapQuadsAutomatically = EditorGUILayout.Toggle(
            "Build Map Quads Automatically",
            buildMapQuadsAutomatically
        );

        EditorGUILayout.Space();

        GUI.enabled = selectedTerrain != null;

        if (GUILayout.Button("Generate Selected Terrain"))
        {
            GenerateMap(selectedTerrain);
        }

        GUI.enabled = true;

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate ALL Terrains"))
        {
            GenerateAllTerrains();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Rebuild Map Quads Only"))
        {
            BuildAllMapQuads();
        }
    }

    // ================================================================
    // GENERATION
    // ================================================================

    private void GenerateAllTerrains()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        if (terrains == null || terrains.Length == 0)
        {
            Debug.LogWarning("No Terrains found in the scene.");
            return;
        }

        foreach (Terrain terrain in terrains)
        {
            GenerateMap(terrain);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        MarkSceneDirty();

        Debug.Log(
            $"Generated {terrains.Length} terrain map(s)."
        );
    }

    private void GenerateMap(Terrain terrain)
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Invalid Terrain.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        resolution = Mathf.Max(16, resolution);
        contourInterval = Mathf.Max(0.01f, contourInterval);

        Directory.CreateDirectory(outputFolder);

        int pixelCount = resolution * resolution;

        float[] heights = new float[pixelCount];
        int[] bands = new int[pixelCount];
        bool[] water = new bool[pixelCount];

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        Vector3 terrainPosition = terrain.transform.position;

        // ============================================================
        // PASS 1: Sample terrain
        // ============================================================

        for (int y = 0; y < resolution; y++)
        {
            float v = (y + 0.5f) / resolution;

            for (int x = 0; x < resolution; x++)
            {
                float u = (x + 0.5f) / resolution;

                float height =
                    terrainData.GetInterpolatedHeight(u, v)
                    + terrainPosition.y;

                int index = y * resolution + x;

                heights[index] = height;

                if (height < minHeight)
                    minHeight = height;

                if (height > maxHeight)
                    maxHeight = height;
            }
        }

        float firstBand =
            Mathf.Floor(minHeight / contourInterval)
            * contourInterval;

        // ============================================================
        // PASS 2: Elevation bands
        // ============================================================

        for (int i = 0; i < pixelCount; i++)
        {
            float height = heights[i];

            water[i] = height <= waterLevel;

            bands[i] = Mathf.FloorToInt(
                (height - firstBand) / contourInterval
            );
        }

        // ============================================================
        // PASS 3: Create texture
        // ============================================================

        Texture2D texture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        Color32[] pixels = new Color32[pixelCount];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = y * resolution + x;

                bool isWater = water[index];

                // ----------------------------------------------------
                // Water
                // ----------------------------------------------------

                if (isWater)
                {
                    if (outlineWater &&
                        IsContourPixel(
                            x,
                            y,
                            bands,
                            water,
                            resolution,
                            contourThicknessPixels
                        ))
                    {
                        pixels[index] = contourColor;
                    }
                    else
                    {
                        pixels[index] = waterColor;
                    }

                    continue;
                }

                // ----------------------------------------------------
                // Land
                // ----------------------------------------------------

                bool contour = IsContourPixel(
                    x,
                    y,
                    bands,
                    water,
                    resolution,
                    contourThicknessPixels
                );

                pixels[index] =
                    contour
                        ? contourColor
                        : landColor;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        // ============================================================
        // SAVE PNG
        // ============================================================

        string safeName = MakeSafeFileName(terrain.name);

        string pngPath = Path.Combine(
            outputFolder,
            safeName + "_Topographic.png"
        );

        File.WriteAllBytes(
            pngPath,
            texture.EncodeToPNG()
        );

        DestroyImmediate(texture);

        AssetDatabase.Refresh();

        ConfigureTextureImport(pngPath);

        // Need to reload the imported texture asset.
        Texture2D textureAsset =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                pngPath
            );

        if (textureAsset == null)
        {
            Debug.LogError(
                $"Could not load generated texture: {pngPath}"
            );

            return;
        }

        // ============================================================
        // BUILD QUAD
        // ============================================================

        if (buildMapQuadsAutomatically)
        {
            BuildMapQuad(
                terrain,
                textureAsset
            );
        }

        Debug.Log(
            $"Generated: {pngPath}\n" +
            $"Terrain: {terrain.name}\n" +
            $"Elevation: {minHeight:F1}m - {maxHeight:F1}m"
        );
    }

    // ================================================================
    // MAP QUAD CREATION
    // ================================================================

    private void BuildAllMapQuads()
    {
        Terrain[] terrains = Terrain.activeTerrains;

        if (terrains == null || terrains.Length == 0)
        {
            Debug.LogWarning("No Terrains found.");
            return;
        }

        foreach (Terrain terrain in terrains)
        {
            string safeName =
                MakeSafeFileName(terrain.name);

            string texturePath =
                Path.Combine(
                    outputFolder,
                    safeName + "_Topographic.png"
                );

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    texturePath
                );

            if (texture == null)
            {
                Debug.LogWarning(
                    $"No generated texture found for {terrain.name}."
                );

                continue;
            }

            BuildMapQuad(
                terrain,
                texture
            );
        }

        AssetDatabase.SaveAssets();

        MarkSceneDirty();

        Debug.Log("Map quads rebuilt.");
    }

    private void BuildMapQuad(
        Terrain terrain,
        Texture2D texture
    )
    {
        if (terrain == null || terrain.terrainData == null)
            return;

        TerrainData terrainData =
            terrain.terrainData;

        Vector3 terrainPosition =
            terrain.transform.position;

        Vector3 terrainSize =
            terrainData.size;

        Transform mapRoot =
            GetOrCreateGameObjectPath(
                mapRootPath
            ).transform;

        string tileName =
            MakeSafeFileName(terrain.name)
            + "_MapTile";

        Transform existing =
            mapRoot.Find(tileName);

        GameObject tile;

        if (existing != null)
        {
            tile = existing.gameObject;
        }
        else
        {
            tile = GameObject.CreatePrimitive(
                PrimitiveType.Quad
            );

            tile.name = tileName;

            Undo.RegisterCreatedObjectUndo(
                tile,
                "Create Map Tile"
            );
        }

        // ============================================================
        // TRANSFORM
        // ============================================================

        tile.transform.SetParent(
            mapRoot,
            true
        );

        // Terrain center.
        float centerX =
            terrainPosition.x
            + terrainSize.x * 0.5f;

        float centerZ =
            terrainPosition.z
            + terrainSize.z * 0.5f;

        float mapY =
            terrainPosition.y
            + quadHeightOffset;

        tile.transform.position =
            new Vector3(
                centerX,
                mapY,
                centerZ
            );

        // Unity Quad starts facing +Z.
        // Rotate it flat so it lies across X/Z.
        tile.transform.rotation =
            Quaternion.Euler(
                90f,
                0f,
                0f
            );

        // Default Unity Quad is 1x1.
        // Scale it to exactly match the Terrain.
        tile.transform.localScale =
            new Vector3(
                terrainSize.x,
                terrainSize.z,
                1f
            );

        // ============================================================
        // LAYER
        // ============================================================

        int mapLayer =
            LayerMask.NameToLayer(mapLayerName);

        if (mapLayer >= 0)
        {
            tile.layer = mapLayer;
        }
        else
        {
            Debug.LogWarning(
                $"Layer '{mapLayerName}' does not exist. " +
                $"Tile '{tile.name}' was not assigned to it."
            );
        }

        // ============================================================
        // MATERIAL
        // ============================================================

        MeshRenderer renderer =
            tile.GetComponent<MeshRenderer>();

        renderer.shadowCastingMode =
            ShadowCastingMode.Off;

        renderer.receiveShadows = false;

        Material material =
            GetOrCreateMaterial(
                terrain,
                texture
            );

        renderer.sharedMaterial = material;

        // ============================================================
        // COLLIDER
        // ============================================================

        MeshCollider collider =
            tile.GetComponent<MeshCollider>();

        if (collider != null)
        {
            DestroyImmediate(collider);
        }

        MarkSceneDirty();
    }

    // ================================================================
    // MATERIAL
    // ================================================================

    private Material GetOrCreateMaterial(
        Terrain terrain,
        Texture2D texture
    )
    {
        string safeName =
            MakeSafeFileName(terrain.name);

        string materialPath =
            Path.Combine(
                outputFolder,
                safeName + "_Topographic.mat"
            );

        materialPath =
            materialPath.Replace(
                "\\",
                "/"
            );

        Material material =
            AssetDatabase.LoadAssetAtPath<Material>(
                materialPath
            );

        if (material == null)
        {
            Shader shader =
                Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader =
                    Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                Debug.LogError(
                    "Could not find a transparent/unlit shader."
                );

                return null;
            }

            material =
                new Material(shader);

            material.name =
                safeName + "_Topographic";

            AssetDatabase.CreateAsset(
                material,
                materialPath
            );
        }

        material.mainTexture =
            texture;

        // White tint so texture colors are unchanged.
        if (material.HasProperty("_Color"))
        {
            material.SetColor(
                "_Color",
                Color.white
            );
        }

        EditorUtility.SetDirty(material);

        return material;
    }

    // ================================================================
    // CONTOURS
    // ================================================================

    private bool IsContourPixel(
        int x,
        int y,
        int[] bands,
        bool[] water,
        int resolution,
        int thickness
    )
    {
        int centerIndex =
            y * resolution + x;

        int centerBand =
            bands[centerIndex];

        bool centerWater =
            water[centerIndex];

        for (int offsetY = -thickness;
             offsetY <= thickness;
             offsetY++)
        {
            for (int offsetX = -thickness;
                 offsetX <= thickness;
                 offsetX++)
            {
                if (offsetX == 0 &&
                    offsetY == 0)
                {
                    continue;
                }

                int nx = x + offsetX;
                int ny = y + offsetY;

                if (nx < 0 ||
                    nx >= resolution ||
                    ny < 0 ||
                    ny >= resolution)
                {
                    continue;
                }

                int neighbourIndex =
                    ny * resolution + nx;

                bool neighbourWater =
                    water[neighbourIndex];

                // Water boundary.
                if (outlineWater &&
                    centerWater != neighbourWater)
                {
                    return true;
                }

                // Don't draw elevation contours in water.
                if (centerWater ||
                    neighbourWater)
                {
                    continue;
                }

                // Elevation boundary.
                if (bands[neighbourIndex] != centerBand)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ================================================================
    // GAMEOBJECT PATH
    // ================================================================

    private GameObject GetOrCreateGameObjectPath(
        string path
    )
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "Map";
        }

        path =
            path.Replace("\\", "/");

        string[] parts =
            path.Split(
                '/',
                System.StringSplitOptions.RemoveEmptyEntries
            );

        GameObject current = null;

        foreach (string part in parts)
        {
            if (current == null)
            {
                // Root-level object.
                current =
                    FindRootGameObject(part);

                if (current == null)
                {
                    current =
                        new GameObject(part);

                    Undo.RegisterCreatedObjectUndo(
                        current,
                        "Create Map Root"
                    );
                }
            }
            else
            {
                Transform child =
                    current.transform.Find(part);

                if (child != null)
                {
                    current =
                        child.gameObject;
                }
                else
                {
                    GameObject newChild =
                        new GameObject(part);

                    Undo.RegisterCreatedObjectUndo(
                        newChild,
                        "Create Map Hierarchy"
                    );

                    newChild.transform.SetParent(
                        current.transform,
                        false
                    );

                    current =
                        newChild;
                }
            }
        }

        return current;
    }

    private GameObject FindRootGameObject(
        string objectName
    )
    {
        Scene scene =
            SceneManager.GetActiveScene();

        GameObject[] rootObjects =
            scene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            if (root.name == objectName)
                return root;
        }

        return null;
    }

    // ================================================================
    // TEXTURE IMPORT
    // ================================================================

    private void ConfigureTextureImport(
        string path
    )
    {
        path =
            path.Replace(
                "\\",
                "/"
            );

        TextureImporter importer =
            AssetImporter.GetAtPath(path)
            as TextureImporter;

        if (importer == null)
            return;

        importer.textureType =
            TextureImporterType.Default;

        importer.alphaIsTransparency =
            true;

        importer.mipmapEnabled =
            false;

        importer.wrapMode =
            TextureWrapMode.Clamp;

        importer.filterMode =
            FilterMode.Bilinear;

        importer.SaveAndReimport();
    }

    // ================================================================
    // HELPERS
    // ================================================================

    private string MakeSafeFileName(
        string value
    )
    {
        foreach (
            char c
            in Path.GetInvalidFileNameChars()
        )
        {
            value =
                value.Replace(
                    c,
                    '_'
                );
        }

        return value;
    }

    private void MarkSceneDirty()
    {
        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );
    }
}

#endif