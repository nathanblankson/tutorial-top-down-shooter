using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update on value in inspector changed
        if (DrawDefaultInspector())
        {
            GenerateMap();
        }

        // Update using button
        if (GUILayout.Button("Generate Map"))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        MapGenerator map = (MapGenerator) target;
        map.GenerateMap();
    }
}
