using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Level Generation Settings")]
    [SerializeField] private int levelWidth = 100;
    [SerializeField] private int levelLength = 100;
    [SerializeField] private float noiseScale = 20f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private int seed = 42;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private bool randomizeSeed = true;

    [Header("Terrain Settings")]
    [SerializeField] private float heightMultiplier = 20f;
    [SerializeField] private AnimationCurve heightCurve;
    [SerializeField] private bool generateMesh = true;

    [Header("Environment Objects")]
    [SerializeField] private Transform environmentParent;
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject[] structurePrefabs;  // Buildings, towers, etc.
    [SerializeField] private GameObject playerPrefab;

    [Header("Placement Rules")]
    [Range(0, 1)] [SerializeField] private float treeDensity = 0.15f;
    [Range(0, 1)] [SerializeField] private float rockDensity = 0.05f;
    [Range(0, 1)] [SerializeField] private float grassDensity = 0.3f;
    [Range(0, 1)] [SerializeField] private float enemyDensity = 0.02f;
    [SerializeField] private int maxStructures = 5;
    [SerializeField] private float minDistanceBetweenStructures = 20f;
    [SerializeField] private float minHeightForStructures = 3f;
    [SerializeField] private float maxHeightForStructures = 15f;
    [SerializeField] private float minDistanceFromEdge = 10f;
    [SerializeField] private float minSlopeForTrees = 0.1f;
    [SerializeField] private float maxSlopeForTrees = 0.7f;
    [SerializeField] private float waterLevel = 2f;
    
    // Internal variables
    private Mesh terrainMesh;
    private Vector3[] vertices;
    private float[,] heightMap;
    private float[,] slopeMap;
    private Vector2[] noiseOffsets;
    private List<Vector3> structurePositions = new List<Vector3>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    private void Start()
    {
        Debug.Log("ProceduralLevelGenerator: Start function called");
        
        if (environmentParent == null)
        {
            GameObject envParent = new GameObject("Environment");
            environmentParent = envParent.transform;
            Debug.Log("ProceduralLevelGenerator: Created Environment parent object");
        }
        
        // Initialize the level
        GenerateLevel();
    }
    
    public void GenerateLevel()
    {
        Debug.Log("ProceduralLevelGenerator: GenerateLevel function called");
        
        // Clean up previous level if it exists
        CleanupLevel();
        
        // Randomize seed if requested
        if (randomizeSeed)
        {
            seed = Random.Range(0, 10000);
            Debug.Log($"ProceduralLevelGenerator: Randomized seed to {seed}");
        }
        
        // Initialize noise offsets
        Random.InitState(seed);
        noiseOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = Random.Range(-100000f, 100000f) + offset.x;
            float offsetY = Random.Range(-100000f, 100000f) + offset.y;
            noiseOffsets[i] = new Vector2(offsetX, offsetY);
        }
        
        // Generate the terrain height map
        GenerateHeightMap();
        
        // Generate terrain mesh if requested
        if (generateMesh)
        {
            GenerateTerrainMesh();
        }
        
        // Calculate slopes across the terrain
        CalculateSlopeMap();
        
        // Place objects according to rules
        PlaceStructures();
        PlacePlayer();
        PlaceEnemies();
        PlaceEnvironmentObjects();
        
        Debug.Log("ProceduralLevelGenerator: Level generation complete");
    }
    
    private void CleanupLevel()
    {
        Debug.Log("ProceduralLevelGenerator: Cleaning up previous level");
        
        // Remove all spawned objects
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        spawnedObjects.Clear();
        structurePositions.Clear();
        
        // Reset terrain mesh if needed
        if (terrainMesh != null)
        {
            terrainMesh = null;
        }
    }
    
    private void GenerateHeightMap()
    {
        Debug.Log("ProceduralLevelGenerator: Generating height map");
        
        heightMap = new float[levelWidth, levelLength];
        
        // Generate the basic noise map
        for (int x = 0; x < levelWidth; x++)
        {
            for (int z = 0; z < levelLength; z++)
            {
                // Initialize variables for octave calculations
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                float normalizer = 0;
                
                // Generate multiple octaves of noise
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (float)x / noiseScale * frequency + noiseOffsets[i].x;
                    float sampleZ = (float)z / noiseScale * frequency + noiseOffsets[i].y;
                    
                    // Get perlin noise value and convert from 0-1 to -1 to 1
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    
                    // Update for next octave
                    normalizer += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                // Normalize the height value
                noiseHeight /= normalizer;
                
                // Apply the height curve for better terrain shaping
                heightMap[x, z] = heightCurve.Evaluate(Mathf.Clamp01((noiseHeight + 1) / 2f));
            }
        }
        
        Debug.Log("ProceduralLevelGenerator: Height map generation complete");
    }
    
    private void GenerateTerrainMesh()
    {
        Debug.Log("ProceduralLevelGenerator: Generating terrain mesh");
        
        // Create a new mesh or get existing
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        
        // Create new mesh
        terrainMesh = new Mesh();
        terrainMesh.name = "Terrain Mesh";
        
        // Generate vertices
        vertices = new Vector3[(levelWidth) * (levelLength)];
        int[] triangles = new int[(levelWidth - 1) * (levelLength - 1) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];
        
        int vertexIndex = 0;
        for (int z = 0; z < levelLength; z++)
        {
            for (int x = 0; x < levelWidth; x++)
            {
                // Set each vertex position with height from the height map
                float height = heightMap[x, z] * heightMultiplier;
                vertices[vertexIndex] = new Vector3(x, height, z);
                
                // Set UV coordinates
                uvs[vertexIndex] = new Vector2((float)x / levelWidth, (float)z / levelLength);
                
                vertexIndex++;
            }
        }
        
        // Create triangles
        int triangleIndex = 0;
        for (int z = 0; z < levelLength - 1; z++)
        {
            for (int x = 0; x < levelWidth - 1; x++)
            {
                int currentVertex = x + z * levelWidth;
                int nextRowVertex = currentVertex + levelWidth;
                
                // First triangle
                triangles[triangleIndex++] = currentVertex;
                triangles[triangleIndex++] = nextRowVertex;
                triangles[triangleIndex++] = currentVertex + 1;
                
                // Second triangle
                triangles[triangleIndex++] = currentVertex + 1;
                triangles[triangleIndex++] = nextRowVertex;
                triangles[triangleIndex++] = nextRowVertex + 1;
            }
        }
        
        // Apply to mesh
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.uv = uvs;
        
        // Recalculate mesh properties
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
        
        // Apply mesh to components
        meshFilter.sharedMesh = terrainMesh;
        meshCollider.sharedMesh = terrainMesh;
        
        Debug.Log("ProceduralLevelGenerator: Terrain mesh generation complete");
    }
    
    private void CalculateSlopeMap()
    {
        Debug.Log("ProceduralLevelGenerator: Calculating slope map");
        
        slopeMap = new float[levelWidth, levelLength];
        
        // Calculate slope for each point by examining neighbors
        for (int x = 1; x < levelWidth - 1; x++)
        {
            for (int z = 1; z < levelLength - 1; z++)
            {
                // Calculate height differences in each direction
                float heightDiffX = Mathf.Abs(heightMap[x + 1, z] - heightMap[x - 1, z]);
                float heightDiffZ = Mathf.Abs(heightMap[x, z + 1] - heightMap[x, z - 1]);
                
                // Calculate slope as the magnitude of the gradient
                slopeMap[x, z] = (heightDiffX + heightDiffZ) / 2f;
            }
        }
        
        // Handle edges separately (simplified approach)
        for (int x = 0; x < levelWidth; x++)
        {
            slopeMap[x, 0] = slopeMap[x, 1];
            slopeMap[x, levelLength - 1] = slopeMap[x, levelLength - 2];
        }
        
        for (int z = 0; z < levelLength; z++)
        {
            slopeMap[0, z] = slopeMap[1, z];
            slopeMap[levelWidth - 1, z] = slopeMap[levelWidth - 2, z];
        }
        
        Debug.Log("ProceduralLevelGenerator: Slope map calculation complete");
    }
    
    private void PlaceStructures()
    {
        Debug.Log("ProceduralLevelGenerator: Placing structures");
        
        if (structurePrefabs == null || structurePrefabs.Length == 0)
        {
            Debug.LogWarning("ProceduralLevelGenerator: No structure prefabs assigned");
            return;
        }
        
        // Try to place the specified number of structures
        int attempts = 0;
        int placedStructures = 0;
        
        while (placedStructures < maxStructures && attempts < maxStructures * 10)
        {
            attempts++;
            
            // Pick a random position that's not too close to the edge
            int x = Random.Range(Mathf.FloorToInt(minDistanceFromEdge), levelWidth - Mathf.FloorToInt(minDistanceFromEdge));
            int z = Random.Range(Mathf.FloorToInt(minDistanceFromEdge), levelLength - Mathf.FloorToInt(minDistanceFromEdge));
            
            // Get height at this position
            float height = heightMap[x, z] * heightMultiplier;
            
            // Check if height is suitable for a structure
            if (height < minHeightForStructures || height > maxHeightForStructures)
            {
                continue;
            }
            
            // Check if position is far enough from water
            if (height < waterLevel + 3f)
            {
                continue;
            }
            
            // Check if position is too close to another structure
            Vector3 position = new Vector3(x, height, z);
            bool tooClose = false;
            
            foreach (Vector3 structurePos in structurePositions)
            {
                if (Vector3.Distance(position, structurePos) < minDistanceBetweenStructures)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose)
            {
                continue;
            }
            
            // Place structure
            GameObject structurePrefab = structurePrefabs[Random.Range(0, structurePrefabs.Length)];
            GameObject structure = Instantiate(structurePrefab, position, Quaternion.identity, environmentParent);
            
            // Random rotation for variety
            structure.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            structurePositions.Add(position);
            spawnedObjects.Add(structure);
            placedStructures++;
            
            Debug.Log($"ProceduralLevelGenerator: Placed structure {placedStructures} at position {position}");
        }
        
        Debug.Log($"ProceduralLevelGenerator: Placed {placedStructures} structures");
    }
    
    private void PlacePlayer()
    {
        Debug.Log("ProceduralLevelGenerator: Placing player");
        
        if (playerPrefab == null)
        {
            Debug.LogWarning("ProceduralLevelGenerator: No player prefab assigned");
            return;
        }
        
        // Try to find a good spot for the player
        int attempts = 0;
        bool playerPlaced = false;
        
        while (!playerPlaced && attempts < 100)
        {
            attempts++;
            
            // Try to place near center-ish of the map
            int centerOffsetX = Random.Range(-levelWidth / 4, levelWidth / 4);
            int centerOffsetZ = Random.Range(-levelLength / 4, levelLength / 4);
            
            int x = levelWidth / 2 + centerOffsetX;
            int z = levelLength / 2 + centerOffsetZ;
            
            x = Mathf.Clamp(x, 5, levelWidth - 5);
            z = Mathf.Clamp(z, 5, levelLength - 5);
            
            // Get height at this position
            float height = heightMap[x, z] * heightMultiplier;
            
            // Check if height is suitable (not underwater)
            if (height < waterLevel + 1f)
            {
                continue;
            }
            
            // Check if not too close to structures
            Vector3 position = new Vector3(x, height + 1f, z);  // Offset slightly above ground
            bool tooClose = false;
            
            foreach (Vector3 structurePos in structurePositions)
            {
                if (Vector3.Distance(position, structurePos) < 5f)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose)
            {
                continue;
            }
            
            // Place player
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            spawnedObjects.Add(player);
            playerPlaced = true;
            
            Debug.Log($"ProceduralLevelGenerator: Placed player at position {position}");
        }
        
        if (!playerPlaced)
        {
            Debug.LogWarning("ProceduralLevelGenerator: Failed to place player after max attempts");
        }
    }
    
    private void PlaceEnemies()
    {
        Debug.Log("ProceduralLevelGenerator: Placing enemies");
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("ProceduralLevelGenerator: No enemy prefabs assigned");
            return;
        }
        
        // Calculate number of enemies based on density and map size
        int totalEnemies = Mathf.FloorToInt(levelWidth * levelLength * enemyDensity / 100f);
        int enemiesPlaced = 0;
        int maxAttempts = totalEnemies * 5;
        int attempts = 0;
        
        while (enemiesPlaced < totalEnemies && attempts < maxAttempts)
        {
            attempts++;
            
            // Pick a random position
            int x = Random.Range(5, levelWidth - 5);
            int z = Random.Range(5, levelLength - 5);
            
            // Get height at this position
            float height = heightMap[x, z] * heightMultiplier;
            
            // Skip underwater positions
            if (height < waterLevel + 0.5f)
            {
                continue;
            }
            
            // Make sure it's not too close to any structure or player spawn
            Vector3 position = new Vector3(x, height + 0.1f, z);  // Small offset from ground
            bool validPosition = true;
            
            // Check distance from structures
            foreach (Vector3 structurePos in structurePositions)
            {
                if (Vector3.Distance(position, structurePos) < 7f)
                {
                    validPosition = false;
                    break;
                }
            }
            
            if (!validPosition)
            {
                continue;
            }
            
            // Place enemy
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, environmentParent);
            
            // Random rotation
            enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            spawnedObjects.Add(enemy);
            enemiesPlaced++;
            
            // Log only occasionally to avoid spamming
            if (enemiesPlaced % 10 == 0 || enemiesPlaced == 1 || enemiesPlaced == totalEnemies)
            {
                Debug.Log($"ProceduralLevelGenerator: Placed enemy {enemiesPlaced}/{totalEnemies}");
            }
        }
        
        Debug.Log($"ProceduralLevelGenerator: Placed {enemiesPlaced} enemies in {attempts} attempts");
    }
    
    private void PlaceEnvironmentObjects()
    {
        Debug.Log("ProceduralLevelGenerator: Placing environment objects");
        
        PlaceObjectsByRule(treePrefabs, treeDensity, TreePlacementRule);
        PlaceObjectsByRule(rockPrefabs, rockDensity, RockPlacementRule);
        PlaceObjectsByRule(grassPrefabs, grassDensity, GrassPlacementRule);
    }
    
    private void PlaceObjectsByRule(GameObject[] prefabs, float density, System.Func<int, int, bool> placementRule)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return;
        }
        
        // Calculate number of objects based on density
        int totalObjects = Mathf.FloorToInt(levelWidth * levelLength * density / 20f);
        int objectsPlaced = 0;
        int attempts = 0;
        int maxAttempts = totalObjects * 3;
        
        while (objectsPlaced < totalObjects && attempts < maxAttempts)
        {
            attempts++;
            
            // Pick a random position
            int x = Random.Range(1, levelWidth - 1);
            int z = Random.Range(1, levelLength - 1);
            
            // Check if this position meets the placement rule
            if (!placementRule(x, z))
            {
                continue;
            }
            
            // Get the height at this position
            float height = heightMap[x, z] * heightMultiplier;
            Vector3 position = new Vector3(x, height, z);
            
            // Place the object
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject obj = Instantiate(prefab, position, Quaternion.identity, environmentParent);
            
            // Add random rotation and slight scale variation
            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float scaleVariation = Random.Range(0.8f, 1.2f);
            obj.transform.localScale *= scaleVariation;
            
            spawnedObjects.Add(obj);
            objectsPlaced++;
            
            // Log progress occasionally
            if (objectsPlaced % 50 == 0 || objectsPlaced == 1 || objectsPlaced == totalObjects)
            {
                Debug.Log($"ProceduralLevelGenerator: Placed {objectsPlaced}/{totalObjects} objects");
            }
        }
    }
    
    private bool TreePlacementRule(int x, int z)
    {
        // Trees need appropriate slope and height
        float height = heightMap[x, z] * heightMultiplier;
        float slope = slopeMap[x, z];
        
        // Trees shouldn't be underwater
        if (height < waterLevel + 0.5f)
        {
            return false;
        }
        
        // Trees need appropriate slope
        if (slope < minSlopeForTrees || slope > maxSlopeForTrees)
        {
            return false;
        }
        
        // Trees shouldn't be too close to structures
        Vector3 position = new Vector3(x, height, z);
        foreach (Vector3 structurePos in structurePositions)
        {
            if (Vector3.Distance(position, structurePos) < 5f)
            {
                return false;
            }
        }
        
        // Add some randomness for natural distribution
        return Random.value < 0.7f;
    }
    
    private bool RockPlacementRule(int x, int z)
    {
        // Rocks tend to appear on steeper slopes
        float height = heightMap[x, z] * heightMultiplier;
        float slope = slopeMap[x, z];
        
        // Rocks can be partially underwater
        if (height < waterLevel - 1f)
        {
            return false;
        }
        
        // Rocks often on steeper terrain
        if (slope < 0.3f && Random.value < 0.7f)
        {
            return false;
        }
        
        // Rocks shouldn't be too close to structures
        Vector3 position = new Vector3(x, height, z);
        foreach (Vector3 structurePos in structurePositions)
        {
            if (Vector3.Distance(position, structurePos) < 3f)
            {
                return false;
            }
        }
        
        // Add some randomness for natural distribution
        return Random.value < 0.6f;
    }
    
    private bool GrassPlacementRule(int x, int z)
    {
        // Grass appears on flatter areas
        float height = heightMap[x, z] * heightMultiplier;
        float slope = slopeMap[x, z];
        
        // Grass shouldn't be underwater
        if (height < waterLevel)
        {
            return false;
        }
        
        // Grass prefers flatter terrain
        if (slope > 0.4f)
        {
            return false;
        }
        
        // Grass can be close to structures
        return Random.value < 0.8f;
    }
    
    public Vector3 GetRandomPositionOnTerrain()
    {
        int x = Random.Range(0, levelWidth);
        int z = Random.Range(0, levelLength);
        float height = heightMap[x, z] * heightMultiplier;
        
        return new Vector3(x, height, z);
    }
    
    // Editor utility for generating previews or debugging
    public void RegenerateInEditor()
    {
        GenerateLevel();
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && vertices != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector3 vertex in vertices)
            {
                Gizmos.DrawSphere(vertex, 0.1f);
            }
        }
        
        // Draw structure positions
        Gizmos.color = Color.red;
        foreach (Vector3 pos in structurePositions)
        {
            Gizmos.DrawWireSphere(pos, minDistanceBetweenStructures / 2f);
        }
    }
}