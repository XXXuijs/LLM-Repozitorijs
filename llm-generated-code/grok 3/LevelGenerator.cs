using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    // Terrain generation variables
    [SerializeField] private int terrainSize = 50; // Grid size (vertices)
    [SerializeField] private float noiseScale = 0.1f; // Perlin noise frequency
    [SerializeField] private float heightScale = 10f; // Max terrain height
    [SerializeField] private float cellSize = 1f; // Distance between vertices
    [SerializeField] private Material terrainMaterial;

    // Prefab placement variables
    [SerializeField] private List<PrefabPlacementRule> placementRules;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float minPrefabDistance = 2f; // Minimum distance between prefabs

    // Components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    [System.Serializable]
    public class PrefabPlacementRule
    {
        public GameObject prefab;
        public float minHeight; // Min terrain height for placement
        public float maxHeight; // Max terrain height for placement
        public float maxSlope; // Max slope angle (degrees) for placement
        public int maxInstances; // Max number of this prefab
    }

    void Start()
    {
        InitializeComponents();
        GenerateTerrain();
        PlacePrefabs();
    }

    private void InitializeComponents()
    {
        // Add required components
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer.material = terrainMaterial;
        Debug.Log("LevelGenerator: Mesh components initialized.");
    }

    private void GenerateTerrain()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralTerrain";

        // Calculate vertices
        Vector3[] vertices = new Vector3[terrainSize * terrainSize];
        Vector2[] uvs = new Vector2[terrainSize * terrainSize];
        int[] triangles = new int[(terrainSize - 1) * (terrainSize - 1) * 6];

        for (int z = 0, i = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                float height = Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * heightScale;
                vertices[i] = new Vector3(x * cellSize, height, z * cellSize);
                uvs[i] = new Vector2(x / (float)terrainSize, z / (float)terrainSize);
                i++;
            }
        }

        // Calculate triangles
        for (int z = 0, t = 0, v = 0; z < terrainSize - 1; z++, v++)
        {
            for (int x = 0; x < terrainSize - 1; x++, v++)
            {
                triangles[t] = v;
                triangles[t + 1] = v + terrainSize;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + terrainSize;
                triangles[t + 5] = v + terrainSize + 1;
                t += 6;
            }
        }

        // Assign mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        Debug.Log($"LevelGenerator: Terrain generated with {vertices.Length} vertices and {triangles.Length / 3} triangles.");
    }

    private void PlacePrefabs()
    {
        List<Vector3> occupiedPositions = new List<Vector3>();
        int totalPrefabsPlaced = 0;

        foreach (PrefabPlacementRule rule in placementRules)
        {
            int instancesPlaced = 0;
            for (int i = 0; i < rule.maxInstances * 2 && instancesPlaced < rule.maxInstances; i++)
            {
                // Random position on terrain
                float x = Random.Range(0f, terrainSize * cellSize);
                float z = Random.Range(0f, terrainSize * cellSize);

                // Raycast to get terrain height and normal
                Ray ray = new Ray(new Vector3(x, 100f, z), Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 200f, terrainLayer))
                {
                    float height = hit.point.y;
                    Vector3 normal = hit.normal;
                    float slopeAngle = Vector3.Angle(normal, Vector3.up);

                    // Check placement conditions
                    if (height >= rule.minHeight && height <= rule.maxHeight && slopeAngle <= rule.maxSlope)
                    {
                        // Check distance to other prefabs
                        bool tooClose = false;
                        foreach (Vector3 pos in occupiedPositions)
                        {
                            if (Vector3.Distance(hit.point, pos) < minPrefabDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (!tooClose)
                        {
                            // Place prefab
                            GameObject prefab = Instantiate(rule.prefab, hit.point, Quaternion.identity, transform);
                            occupiedPositions.Add(hit.point);
                            instancesPlaced++;
                            totalPrefabsPlaced++;
                            Debug.Log($"LevelGenerator: Placed {rule.prefab.name} at {hit.point}, height: {height}, slope: {slopeAngle} degrees.");
                        }
                        else
                        {
                            Debug.Log($"LevelGenerator: Skipped {rule.prefab.name} placement at {hit.point} - too close to another prefab.");
                        }
                    }
                    else
                    {
                        Debug.Log($"LevelGenerator: Skipped {rule.prefab.name} placement at {hit.point} - height: {height}, slope: {slopeAngle} out of bounds.");
                    }
                }
            }
            Debug.Log($"LevelGenerator: Placed {instancesPlaced} instances of {rule.prefab.name}.");
        }
        Debug.Log($"LevelGenerator: Total prefabs placed: {totalPrefabsPlaced}.");
    }
}