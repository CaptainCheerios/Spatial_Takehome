using System;
using UnityEngine;

public class CaptureGameMode : MonoBehaviour
{
    public static CaptureGameMode Instance { get; private set; }

    public int spawned;
    public int captured;
    public bool allContainersBroken;
    public float timerLength = 120f;

    private void Awake()
    {
        if (Instance)
            Destroy(this);
        Instance = this;
    }

    public void CountAllCreatureSpawns()
    {
        int maxSpawns = 0;
        var grabbableObjects =
            FindObjectsByType<GrabbableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GrabbableObject grabbableObject in grabbableObjects)
        {
            maxSpawns+= grabbableObject.maxCreatureCount;
        }
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