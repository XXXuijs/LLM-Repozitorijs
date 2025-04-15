using UnityEngine;

public class PrefabPlacer : TerrainGenerator
{
    public GameObject prefabToPlace; // Store the reference to the prefab to place
    public float prefabSpacing = 20f; // Distance between placed prefabs

    private void PlacePrefabs()
    {
        Debug.Log("Placing prefabs on terrain.");

        for (int i = 0; i < terrainResolution - 1; i++)
        {
            for (int j = 0; j < terrainResolution - 1; j++)
            {
                float x = (i * terrainSize) / terrainResolution;
                float z = (j * terrainSize) / terrainResolution;

                if (Mathf.PerlinNoise(x * noiseScale, z * noiseScale) > 0.5f)
                {
                    Vector3 prefabPosition = new Vector3(-scale + x, vertices[i + j * terrainResolution].y, -scale + z);
                    Instantiate(prefabToPlace, prefabPosition, Quaternion.identity);
                }
            }
        }
    }
}
