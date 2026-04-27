using System;
using UnityEngine;

public class CreatureAndSpawnerCounter : MonoBehaviour
{
    public static CreatureAndSpawnerCounter Instance { get; private set; }

    public int spawned { get; private set; } = 0;
    public int captured { get;private set; } = 0;
    public bool allContainersBroken {get ; private set;} = false;
    public int totalCreatures { get; private set; }= 0;
    public int totalGrabbableObjects { get; private set; } = 0;
    public int brokenGrabbableObjects { get; private set; } = 0;
    
    public event Action OnAllCaptured;
    public event Action OnCreatureCaptured;

    public void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }  
        Instance = this;
        CountAllCreatureSpawns();
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
        OnCreatureCaptured?.Invoke();
        if(captured == totalCreatures)
            OnAllCaptured?.Invoke();
    }

    public void GrabbableBroken()
    {
        brokenGrabbableObjects++;
        if(brokenGrabbableObjects == totalGrabbableObjects)
            allContainersBroken = true;
    }
}