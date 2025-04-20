using UnityEngine;
using System.Collections.Generic; // Required for Lists

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private int seed = 0; // Seed for randomization
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearPreviousLevel = true; // Clear objects from previous generation

    [Header("Terrain Generation")]
    [SerializeField] private int mapWidth = 100; // Size in grid units
    [SerializeField] private int mapDepth = 100;
    [SerializeField] private float terrainScale = 1.0f; // Grid unit size in world space
    [SerializeField] private float noiseScale = 20f; // Controls the frequency of Perlin noise features
    [SerializeField] private int octaves = 4; // Number of noise layers for detail
    [Range(0f, 1f)]
    [SerializeField] private float persistence = 0.5f; // How much detail is added or removed at each octave
    [SerializeField] private float lacunarity = 2.0f; // How much detail increases with each octave
    [SerializeField] private Vector2 noiseOffset = Vector2.zero; // Allows shifting the noise pattern
    [SerializeField] private float terrainHeightMultiplier = 15f; // Max height variation
    [SerializeField] private AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1); // Remaps height values
    [SerializeField] private Material terrainMaterial; // Material for the generated terrain
    [SerializeField] private PhysicsMaterial terrainPhysicsMaterial; // Optional physics material
    [SerializeField] private LayerMask terrainLayer; // Layer for the generated terrain (used for raycasting)

    [Header("Prefab Placement")]
    [SerializeField] private PrefabPlacementRule[] placementRules;
    [SerializeField] private LayerMask placementOverlapLayer; // Layer(s) to check for overlaps (usually includes terrain + other placed objects)

    // --- Private Members ---
    private GameObject terrainObject;
    private GameObject prefabsContainer;
    private List<Vector3> placedObjectPositions = new List<Vector3>(); // For simple overlap check

    // Helper struct/class for defining placement rules in the Inspector
    [System.Serializable]
    public class PrefabPlacementRule
    {
        public string ruleName = "New Rule";
        public GameObject prefab;
        [Tooltip("Number of attempts to place this prefab.")]
        public int spawnAttempts = 100; // How many times to try placing this prefab
        [Range(0f, 1f)]
        [Tooltip("Chance (0-1) of actually spawning per successful placement check.")]
        public float spawnProbability = 0.8f; // Chance to spawn if rules pass

        [Header("Placement Constraints")]
        [Range(0f, 90f)] public float minSlope = 0f; // Minimum slope angle in degrees
        [Range(0f, 90f)] public float maxSlope = 30f; // Maximum slope angle in degrees
        public float minHeight = -Mathf.Infinity; // Minimum terrain height
        public float maxHeight = Mathf.Infinity; // Maximum terrain height
        [Tooltip("Minimum distance to other placed objects.")]
        public float placementClearance = 1.5f; // Radius to check for overlaps

        [Header("Placement Details")]
        public bool alignToSurfaceNormal = true; // Align Y axis with terrain normal?
        public bool randomYRotation = true; // Randomize rotation around Y axis?
    }

    // --- Methods ---

    void Start()
    {
        Debug.Log("ProceduralLevelGenerator: Start called.");
        if (generateOnStart)
        {
            GenerateLevel();
        }
    }

    [ContextMenu("Generate Level")] // Allows triggering generation from Inspector right-click
    void GenerateLevelEditor()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Debug.Log($"ProceduralLevelGenerator: Starting level generation with Seed: {seed}");

        // Initialize Random State with Seed
        Random.InitState(seed);

        // --- Clear Previous (Optional) ---
        if (clearPreviousLevel)
        {
            ClearLevel();
        }

        // --- Generate Terrain ---
        Debug.Log("ProceduralLevelGenerator: Generating height map...");
        float[,] heightMap = GenerateHeightMap();
        Debug.Log("ProceduralLevelGenerator: Height map generated. Generating terrain mesh...");
        terrainObject = GenerateTerrainMesh(heightMap);
        Debug.Log($"ProceduralLevelGenerator: Terrain mesh generated and assigned to '{terrainObject.name}'.");

        // --- Place Prefabs ---
        Debug.Log("ProceduralLevelGenerator: Placing prefabs...");
        PlacePrefabs();
        Debug.Log("ProceduralLevelGenerator: Prefab placement finished.");

        stopwatch.Stop();
        Debug.Log($"ProceduralLevelGenerator: Level generation finished in {stopwatch.ElapsedMilliseconds} ms.");

        // --- Manual Step Reminder ---
        Debug.LogWarning("ProceduralLevelGenerator: Level generated. Remember to BAKE NAVIGATION manually ('Window -> AI -> Navigation -> Bake') if NavMesh is needed for AI!");
    }

    void ClearLevel()
    {
        Debug.Log("ProceduralLevelGenerator: Clearing previous level objects...");
        // Find and destroy existing terrain and prefab containers created by this script
        // A safer way is to keep references or find by specific names/tags
        GameObject existingTerrain = GameObject.Find("GeneratedTerrain"); // Example name
        if (existingTerrain != null) DestroyImmediate(existingTerrain); // Use DestroyImmediate in Editor

        GameObject existingPrefabs = GameObject.Find("PrefabsContainer"); // Example name
        if (existingPrefabs != null) DestroyImmediate(existingPrefabs);

        placedObjectPositions.Clear(); // Clear placement tracking list
        terrainObject = null;
        prefabsContainer = null;
         Debug.Log("ProceduralLevelGenerator: Previous level cleared.");
    }


    float[,] GenerateHeightMap()
    {
        float[,] map = new float[mapWidth, mapDepth];
        // Use seed for unique offsets for this generation run
        Vector2 seedOffset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f)) + noiseOffset;

        float maxPossibleHeight = 0; // For normalization (multi-octave can exceed 1)
        float amplitude = 1;
        float frequency = 1;

        // Calculate max possible height for normalization later
        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }


        // Generate noise values
        for (int y = 0; y < mapDepth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                // Calculate noise with multiple octaves
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x / (float)mapWidth * noiseScale + seedOffset.x + noiseOffset.x) * frequency;
                    float sampleY = (y / (float)mapDepth * noiseScale + seedOffset.y + noiseOffset.y) * frequency;

                    // Use Unity's PerlinNoise - returns value between 0 and 1
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence; // Amplitude decreases each octave
                    frequency *= lacunarity; // Frequency increases each octave
                }

                // Normalize height based on max possible height from octaves
                // This prevents values from going way out of expected range if persistence is high
                map[x, y] = Mathf.Clamp01(noiseHeight / maxPossibleHeight);

                // Apply height curve for finer control over terrain features
                map[x, y] = heightCurve.Evaluate(map[x, y]);

                // Debug.Log($"HeightMap[{x},{y}] = {map[x, y]}"); // Very spammy
            }
        }
         Debug.Log($"ProceduralLevelGenerator: Calculated height map ({mapWidth}x{mapDepth}).");
        return map;
    }


    GameObject GenerateTerrainMesh(float[,] heightMap)
    {
        Debug.Log("ProceduralLevelGenerator: Starting mesh data generation.");
        int width = heightMap.GetLength(0);
        int depth = heightMap.GetLength(1);

        Mesh mesh = new Mesh();
        mesh.name = "Generated Terrain Mesh";

        int vertexCount = width * depth;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        // Triangle count: (width-1) * (depth-1) squares, 2 triangles per square, 3 indices per triangle
        int[] triangles = new int[(width - 1) * (depth - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        // Create vertices and UVs
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float height = heightMap[x, z] * terrainHeightMultiplier;
                vertices[vertexIndex] = new Vector3(x * terrainScale, height, z * terrainScale);
                uv[vertexIndex] = new Vector2(x / (float)width, z / (float)depth);

                // Create triangles (except for last row/column)
                if (x < width - 1 && z < depth - 1)
                {
                    // Triangle 1 (Bottom Left Square)
                    triangles[triangleIndex + 0] = vertexIndex;              // Bottom Left
                    triangles[triangleIndex + 1] = vertexIndex + width;      // Top Left
                    triangles[triangleIndex + 2] = vertexIndex + 1;          // Bottom Right

                    // Triangle 2 (Top Right Square)
                    triangles[triangleIndex + 3] = vertexIndex + 1;          // Bottom Right
                    triangles[triangleIndex + 4] = vertexIndex + width;      // Top Left
                    triangles[triangleIndex + 5] = vertexIndex + width + 1;  // Top Right

                    triangleIndex += 6;
                }
                vertexIndex++;
            }
        }
         Debug.Log($"ProceduralLevelGenerator: Generated {vertexCount} vertices and {triangleIndex / 3} triangles.");

        // Assign data to mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        Debug.Log("ProceduralLevelGenerator: Recalculating mesh normals and bounds.");
        mesh.RecalculateNormals(); // Important for lighting and slope calculation
        mesh.RecalculateBounds();

        // Create GameObject for the terrain
        GameObject terrainGO = new GameObject("GeneratedTerrain");
        terrainGO.transform.position = Vector3.zero; // Or adjust position as needed
        terrainGO.layer = LayerMask.NameToLayer(LayerMask.LayerToName(terrainLayer.value)); // Assign layer

        MeshFilter meshFilter = terrainGO.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = terrainGO.AddComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;

        MeshCollider meshCollider = terrainGO.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh; // Assign the generated mesh for collisions
        meshCollider.material = terrainPhysicsMaterial; // Assign optional physics material

         Debug.Log($"ProceduralLevelGenerator: Created terrain GameObject '{terrainGO.name}' with MeshFilter, MeshRenderer, MeshCollider.");
        return terrainGO;
    }


    void PlacePrefabs()
    {
        if (terrainObject == null)
        {
            Debug.LogError("ProceduralLevelGenerator: Cannot place prefabs, terrain object is missing!");
            return;
        }
        if (placementRules == null || placementRules.Length == 0)
        {
            Debug.LogWarning("ProceduralLevelGenerator: No prefab placement rules defined.");
            return;
        }

        // Create container for organization
        prefabsContainer = new GameObject("PrefabsContainer");
        placedObjectPositions.Clear(); // Reset list for this generation run

        float maxTerrainHeight = terrainHeightMultiplier; // Approximate max height

        // Iterate through each placement rule
        foreach (PrefabPlacementRule rule in placementRules)
        {
            if (rule.prefab == null)
            {
                Debug.LogWarning($"ProceduralLevelGenerator: Skipping rule '{rule.ruleName}' because prefab is not assigned.");
                continue;
            }

            Debug.Log($"ProceduralLevelGenerator: Processing placement rule '{rule.ruleName}' for prefab '{rule.prefab.name}'. Attempts: {rule.spawnAttempts}");
            int successfulPlacements = 0;

            // Attempt to place the prefab 'spawnAttempts' times
            for (int i = 0; i < rule.spawnAttempts; i++)
            {
                // 1. Choose Random Location Candidate
                float randomX = Random.Range(0, mapWidth * terrainScale);
                float randomZ = Random.Range(0, mapDepth * terrainScale);
                // Start raycast from above the max possible terrain height
                Vector3 rayStart = new Vector3(randomX, maxTerrainHeight + 10f, randomZ);

                RaycastHit hit;
                // 2. Raycast Down onto Terrain Layer
                if (Physics.Raycast(rayStart, Vector3.down, out hit, maxTerrainHeight + 20f, terrainLayer))
                {
                     // Debug.Log($"Raycast hit at {hit.point}");
                    // 3. Check Placement Rules
                    float terrainHeight = hit.point.y;
                    float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);

                    bool heightRulePassed = terrainHeight >= rule.minHeight && terrainHeight <= rule.maxHeight;
                    bool slopeRulePassed = slopeAngle >= rule.minSlope && slopeAngle <= rule.maxSlope;

                    if (heightRulePassed && slopeRulePassed)
                    {
                        // Debug.Log($"Rules passed: Height={terrainHeight}, Slope={slopeAngle}");
                        // 4. Check Overlap / Clearance
                        bool overlapFound = false;
                        // Check against previously placed objects (simple radius check)
                        foreach (Vector3 placedPos in placedObjectPositions)
                        {
                            if (Vector3.Distance(hit.point, placedPos) < rule.placementClearance)
                            {
                                overlapFound = true;
                                // Debug.Log($"Overlap detected with existing object near {placedPos}");
                                break;
                            }
                        }
                        // Optional: More robust overlap check using Physics.OverlapSphere
                         // Collider[] overlaps = Physics.OverlapSphere(hit.point, rule.placementClearance, placementOverlapLayer);
                         // if (overlaps.Length > 0) {
                         //     // Check if overlap is only with terrain itself or with other important objects
                         //     overlapFound = true; // Simplified for now
                         // }

                        if (!overlapFound)
                        {
                            // 5. Check Spawn Probability
                            if (Random.value <= rule.spawnProbability) // Random.value is 0.0 to 1.0
                            {
                                // 6. Place Prefab
                                Vector3 placementPosition = hit.point;
                                Quaternion placementRotation = Quaternion.identity;

                                if (rule.alignToSurfaceNormal)
                                {
                                    // Align prefab's up direction with the terrain normal
                                    placementRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                                }
                                if (rule.randomYRotation)
                                {
                                    // Apply random rotation around the Y axis (local up)
                                    placementRotation *= Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                                }

                                Debug.Log($"ProceduralLevelGenerator: Placing '{rule.prefab.name}' at {placementPosition.ToString("F2")}. Height={terrainHeight:F2}, Slope={slopeAngle:F2}");
                                GameObject placedInstance = Instantiate(rule.prefab, placementPosition, placementRotation, prefabsContainer.transform);
                                placedObjectPositions.Add(placementPosition); // Track position for overlap check
                                successfulPlacements++;
                            }
                             else
                            {
                                // Debug.Log("Spawn probability check failed.");
                            }
                        }
                        // else { Debug.Log("Overlap check failed."); }
                    }
                    // else { Debug.Log($"Rule check failed: HeightOK={heightRulePassed}, SlopeOK={slopeRulePassed}"); }
                }
                // else { Debug.Log($"Raycast from {rayStart} missed terrain."); }
            } // End of spawn attempts loop
            Debug.Log($"ProceduralLevelGenerator: Rule '{rule.ruleName}' finished. Successfully placed {successfulPlacements} instances.");
        } // End of rules loop
    }
}