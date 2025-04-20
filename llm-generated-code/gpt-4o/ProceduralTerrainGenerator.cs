using UnityEngine;

public class ProceduralTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int width = 50;
    public int depth = 50;
    public float scale = 10f;
    public float heightMultiplier = 5f;
    public GameObject terrainBlockPrefab;

    [Header("Prefab Placement")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    public GameObject enemyPrefab;
    public float treeHeightMin = 2f;
    public float treeHeightMax = 4f;
    public float rockHeightMin = 0f;
    public float rockHeightMax = 2f;
    public float enemySpawnChance = 0.05f; // 5%

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        Debug.Log("GenerateTerrain(): Starting procedural generation...");

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float y = Mathf.PerlinNoise(x / scale, z / scale) * heightMultiplier;

                Vector3 blockPos = new Vector3(x, y, z);
                GameObject block = Instantiate(terrainBlockPrefab, blockPos, Quaternion.identity, transform);
                Debug.Log($"GenerateTerrain(): Created terrain block at {blockPos}");

                PlacePrefabAtHeight(blockPos, y);
            }
        }

        Debug.Log("GenerateTerrain(): Terrain generation completed.");
    }

    void PlacePrefabAtHeight(Vector3 position, float height)
    {
        float rand = Random.value;

        if (height >= treeHeightMin && height <= treeHeightMax && rand < 0.2f)
        {
            int index = Random.Range(0, treePrefabs.Length);
            Instantiate(treePrefabs[index], position + Vector3.up, Quaternion.identity, transform);
            Debug.Log($"PlacePrefabAtHeight(): Placed tree at {position}");
        }
        else if (height >= rockHeightMin && height <= rockHeightMax && rand < 0.1f)
        {
            int index = Random.Range(0, rockPrefabs.Length);
            Instantiate(rockPrefabs[index], position + Vector3.up * 0.5f, Quaternion.identity, transform);
            Debug.Log($"PlacePrefabAtHeight(): Placed rock at {position}");
        }
        else if (rand < enemySpawnChance)
        {
            Instantiate(enemyPrefab, position + Vector3.up * 1.2f, Quaternion.identity, transform);
            Debug.Log($"PlacePrefabAtHeight(): Spawned enemy at {position}");
        }
    }
}
