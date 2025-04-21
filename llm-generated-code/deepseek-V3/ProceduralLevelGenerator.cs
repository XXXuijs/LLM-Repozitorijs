using UnityEngine;
using System.Collections.Generic;

public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Terrain Generation Settings")]
    [SerializeField] private int width = 100;
    [SerializeField] private int depth = 100;
    [SerializeField] private float scale = 20f;
    [SerializeField] private float heightMultiplier = 10f;
    [SerializeField] private int seed = 0;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private bool useHeightMap = false;
    [SerializeField] private Material terrainMaterial;

    [Header("Prefab Placement Rules")]
    [SerializeField] private List<PrefabPlacementRule> placementRules = new List<PrefabPlacementRule>();

    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float safeSpawnRadius = 5f;

    private Terrain generatedTerrain;
    private float[,] heightValues;

    private void Start()
    {
        GenerateLevel();
        Debug.Log("[LevelGenerator] Level generation completed");
    }

    public void GenerateLevel()
    {
        // Initialize random seed if not using fixed seed
        if (seed == 0) seed = Random.Range(1, 99999);
        Random.InitState(seed);
        Debug.Log($"[LevelGenerator] Generating level with seed: {seed}");

        // Create terrain data
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, heightMultiplier, depth);

        // Generate heightmap
        heightValues = new float[width, depth];
        GenerateHeightmap(terrainData);

        // Create terrain game object
        GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
        terrainObj.transform.position = Vector3.zero;
        generatedTerrain = terrainObj.GetComponent<Terrain>();
        generatedTerrain.materialTemplate = terrainMaterial;

        // Place prefabs
        PlacePrefabs();

        // Spawn player
        SpawnPlayer();

        Debug.Log($"[LevelGenerator] Generated terrain with {width}x{depth} dimensions");
    }

    private void GenerateHeightmap(TerrainData terrainData)
    {
        if (useHeightMap && heightMap != null)
        {
            Debug.Log("[LevelGenerator] Generating heightmap from texture");
            GenerateFromHeightmapTexture(terrainData);
        }
        else
        {
            Debug.Log("[LevelGenerator] Generating heightmap with Perlin noise");
            GeneratePerlinNoiseHeightmap(terrainData);
        }
    }

    private void GeneratePerlinNoiseHeightmap(TerrainData terrainData)
    {
        float offsetX = Random.Range(0f, 9999f);
        float offsetZ = Random.Range(0f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float zCoord = (float)z / depth * scale + offsetZ;

                heightValues[x, z] = Mathf.PerlinNoise(xCoord, zCoord);
            }
        }

        terrainData.SetHeights(0, 0, heightValues);
    }

    private void GenerateFromHeightmapTexture(TerrainData terrainData)
    {
        if (heightMap.width != width || heightMap.height != depth)
        {
            Debug.LogWarning("[LevelGenerator] Heightmap dimensions don't match terrain size. Resizing...");
            Texture2D resized = ResizeTexture(heightMap, width, depth);
            heightMap = resized;
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                heightValues[x, z] = heightMap.GetPixel(x, z).grayscale;
            }
        }

        terrainData.SetHeights(0, 0, heightValues);
    }

    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    private void PlacePrefabs()
    {
        Debug.Log($"[LevelGenerator] Placing {placementRules.Count} prefab types");

        foreach (PrefabPlacementRule rule in placementRules)
        {
            if (rule.prefab == null) continue;

            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = rule.maxCount * 10; // Prevent infinite loops

            while (placedCount < rule.maxCount && attempts < maxAttempts)
            {
                attempts++;
                Vector3 position = GetRandomPosition();

                // Check height conditions
                float height = GetTerrainHeightAt(position);
                if (height < rule.minHeight || height > rule.maxHeight) continue;

                // Check slope conditions
                float slope = GetTerrainSteepnessAt(position);
                if (slope < rule.minSlope || slope > rule.maxSlope) continue;

                // Check safe spawn distance
                if (Vector3.Distance(position, Vector3.zero) < safeSpawnRadius) continue;

                // Check other placement rules
                if (!CanPlaceAtPosition(position, rule.placementRadius, rule.requiredTags)) continue;

                // Instantiate with random rotation
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                if (rule.alignToNormal)
                {
                    rotation = Quaternion.FromToRotation(Vector3.up, GetTerrainNormalAt(position)) * rotation;
                }

                GameObject instance = Instantiate(rule.prefab, position, rotation);
                instance.transform.parent = transform;
                placedCount++;

                Debug.Log($"[LevelGenerator] Placed {rule.prefab.name} at {position}");
            }

            Debug.Log($"[LevelGenerator] Placed {placedCount}/{rule.maxCount} {rule.prefab.name} objects");
        }
    }

    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(0, width);
        float z = Random.Range(0, depth);
        float y = GetTerrainHeightAt(new Vector3(x, 0, z));
        return new Vector3(x, y, z);
    }

    private float GetTerrainHeightAt(Vector3 position)
    {
        if (generatedTerrain == null) return 0f;
        return generatedTerrain.SampleHeight(position);
    }

    private float GetTerrainSteepnessAt(Vector3 position)
    {
        if (generatedTerrain == null) return 0f;
        return generatedTerrain.terrainData.GetSteepness(
            position.x / terrainData.size.x,
            position.z / terrainData.size.z);
    }

    private Vector3 GetTerrainNormalAt(Vector3 position)
    {
        if (generatedTerrain == null) return Vector3.up;
        return generatedTerrain.terrainData.GetInterpolatedNormal(
            position.x / terrainData.size.x,
            position.z / terrainData.size.z);
    }

    private bool CanPlaceAtPosition(Vector3 position, float radius, string[] requiredTags)
    {
        // Check for nearby objects with required tags
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (Collider col in colliders)
        {
            bool hasRequiredTag = false;
            foreach (string tag in requiredTags)
            {
                if (col.CompareTag(tag))
                {
                    hasRequiredTag = true;
                    break;
                }
            }

            if (!hasRequiredTag && requiredTags.Length > 0)
            {
                return false;
            }
        }
        return true;
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 spawnPosition = Vector3.zero;
        bool foundPosition = false;
        int attempts = 0;

        // Find a safe spawn position
        while (!foundPosition && attempts < 100)
        {
            attempts++;
            Vector2 randomCircle = Random.insideUnitCircle * safeSpawnRadius;
            spawnPosition = new Vector3(
                width * 0.5f + randomCircle.x,
                0,
                depth * 0.5f + randomCircle.y
            );

            spawnPosition.y = GetTerrainHeightAt(spawnPosition) + 2f; // Add player height offset

            // Check if position is valid
            if (Physics.OverlapSphere(spawnPosition, 1f).Length == 0)
            {
                foundPosition = true;
            }
        }

        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[LevelGenerator] Player spawned at {spawnPosition}");
    }

    private void OnDrawGizmosSelected()
    {
        if (generatedTerrain != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                new Vector3(width * 0.5f, heightMultiplier * 0.5f, depth * 0.5f),
                new Vector3(width, heightMultiplier, depth)
            );
        }
    }
}

[System.Serializable]
public class PrefabPlacementRule
{
    public GameObject prefab;
    public int maxCount = 10;
    public float placementRadius = 1f;
    public float minHeight = 0f;
    public float maxHeight = 1f;
    public float minSlope = 0f;
    public float maxSlope = 30f;
    public bool alignToNormal = true;
    public string[] requiredTags = new string[0];
}