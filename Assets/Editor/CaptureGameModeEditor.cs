using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaptureGameMode))]
public class CaptureGameModeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        CaptureGameMode gameMode = (CaptureGameMode)target;
        if (GUILayout.Button("Count All Creature Spawns"))
            gameMode.CountAllCreatureSpawns();
    }
}