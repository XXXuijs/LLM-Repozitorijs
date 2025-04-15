using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public float terrainSize = 100f;
    public int terrainResolution = 256;
    public float noiseScale = 0.1f;
    public float power = 2f;
    public float offset = 0f;

    private void GenerateTerrain()
    {
        Debug.Log("Generating terrain.");

        // Create a new plane for the terrain
        GameObject terrainObject = new GameObject("Terrain");
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[terrainResolution * terrainResolution];
        int[] triangles = new int[terrainResolution * terrainResolution];

        float scale = terrainSize / 2f;

        for (int i = 0; i < terrainResolution; i++)
        {
            for (int j = 0; j < terrainResolution; j++)
            {
                float x = (i * terrainSize) / terrainResolution;
                float y = (j * terrainSize) / terrainResolution;
                float z = Mathf.PerlinNoise(x * noiseScale, y * noiseScale) * power + offset;

                vertices[i + j * terrainResolution] = new Vector3(-scale + x, z, -scale + y);
            }
        }

        for (int i = 0; i < (terrainResolution - 1); i++)
        {
            for (int j = 0; j < (terrainResolution - 1); j++)
            {
                int vertexIndex = i + j * terrainResolution;
                triangles[vertexIndex * 6 + 0] = vertexIndex;
                triangles[vertexIndex * 6 + 1] = vertexIndex + terrainResolution;
                triangles[vertexIndex * 6 + 2] = vertexIndex + terrainResolution + 1;
                triangles[vertexIndex * 6 + 3] = vertexIndex + terrainResolution;
                triangles[vertexIndex * 6 + 4] = vertexIndex + 1;
                triangles[vertexIndex * 6 + 5] = vertexIndex + terrainResolution + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Create a new MeshFilter and assign the generated mesh to it
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Create a new MeshRenderer and set its material
        MeshRenderer meshRenderer = terrainObject.AddComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Self-Illumin/Specular"));
        meshRenderer.material = material;
    }
}
