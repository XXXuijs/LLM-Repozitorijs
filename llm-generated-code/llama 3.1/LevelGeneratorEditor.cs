using UnityEngine;
using UnityEditor;

public class LevelGeneratorEditor : EditorWindow
{
    private TerrainGenerator terrainGenerator;
    private PrefabPlacer prefabPlacer;

    [MenuItem("Tools/Level Generator")]
    public static void ShowLevelGenerator()
    {
        GetWindow<LevelGeneratorEditor>("Level Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Terrain Generation");
        terrainGenerator.terrainSize = EditorGUILayout.FloatField("Terrain Size:", terrainGenerator.terrainSize);
        terrainGenerator.terrainResolution = EditorGUILayout.IntField("Terrain Resolution:", terrainGenerator.terrainResolution);
        terrainGenerator.noiseScale = EditorGUILayout.FloatField("Noise Scale:", terrainGenerator.noiseScale);
        terrainGenerator.power = EditorGUILayout.FloatField("Power:", terrainGenerator.power);
        terrainGenerator.offset = EditorGUILayout.FloatField("Offset:", terrainGenerator.offset);

        EditorGUILayout.LabelField("Prefab Placement");
        prefabPlacer.prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Prefab to Place:", prefabPlacer.prefabToPlace, typeof(GameObject), false);
        prefabPlacer.prefabSpacing = EditorGUILayout.FloatField("Prefab Spacing:", prefabPlacer.prefabSpacing);

        if (GUILayout.Button("Generate Level"))
        {
            GenerateLevel();
        }
    }

    private void GenerateLevel()
    {
        terrainGenerator.GenerateTerrain();
        prefabPlacer.PlacePrefabs();
    }
}
