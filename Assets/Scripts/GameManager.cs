using System;
using UnityEngine;

public class CaptureGameMode : MonoBehaviour
{
    public static CaptureGameMode Instance { get; private set; }

    public int spawned;
    public int captured;
    public bool allContainersBroken;
    public float timerLength = 120f;

    public int totalCreatures = 0;
    public int totalGrabbableObjects = 0;

    private void Awake()
    {
        if (Instance)
            Destroy(this);
        Instance = this;
    }

    public void CountAllCreatureSpawns()
    {
        var grabbableObjects =
            FindObjectsByType<GrabbableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GrabbableObject grabbableObject in grabbableObjects)
        {
            totalCreatures+= grabbableObject.maxCreatureCount;
        }
        totalGrabbableObjects = grabbableObjects.Length;
    }

    public void RegisterCreature()
    {
        spawned++;
    }

    public void Captured()
    {
        captured++;
    }
}