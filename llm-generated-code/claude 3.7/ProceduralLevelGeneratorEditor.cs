using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralLevelGenerator))]
public class ProceduralLevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ProceduralLevelGenerator generator = (ProceduralLevelGenerator)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Preview"))
        {
            generator.RegenerateInEditor();
        }
        
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Level"))
            {
                generator.GenerateLevel();
            }
        }
    }
}