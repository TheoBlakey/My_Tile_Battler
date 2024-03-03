using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexTileBoardGenerator), true)]
public class HexTileBoardEditor : Editor
{
    HexTileBoardGenerator generator;

    private void Awake()
    {
        generator = (HexTileBoardGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Hex Map and Teams"))
        {
            generator.GenerateMap();
        }

        if (GUILayout.Button("Clear Map"))
        {
            generator.ClearMap();
        }

        //if (GUILayout.Button("Create Units"))
        //{
        //    generator.CreateUnits();
        //}
    }
}
