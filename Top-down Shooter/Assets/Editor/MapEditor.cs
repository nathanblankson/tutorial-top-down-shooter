using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // TODO: does not work outside editor e.g. when ctrl + z
        if (GUI.changed)
        {
            MapGenerator map = (MapGenerator) target;
            map.GenerateMap();
        }
    }
}
