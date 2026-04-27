using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreatureAndSpawnerCounter))]
public class CreatureAndSpawnerCounterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        CreatureAndSpawnerCounter creatureAndSpawnerCounter = (CreatureAndSpawnerCounter)target;
        if (GUILayout.Button("Count All Creature Spawns"))
            creatureAndSpawnerCounter.CountAllCreatureSpawns();
    }
}