using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    [Header("Creature Pool")]
    [SerializeField] private Creature creaturePrefab;
    [SerializeField] private int creatureMaxCapactiy = 240;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, -300, 0);
    
    private ObjectPool<Creature> creaturePool;
    
    private Dictionary<int, Creature> creatureActiveMap = new Dictionary<int, Creature>(240);
    
    
    private void Awake()
    {
        if (Instance)
            Destroy(this);
        Instance = this;
        
        //Create the pool
        int creatureID = 0;
        creaturePool = new ObjectPool<Creature>(createFunc: () =>
            {
                var creature = Instantiate(creaturePrefab, transform);
                creature.gameObject.SetActive(false);
                creature.creatureIndex = creatureID;
                creatureID++;
                return creature;
            },
            actionOnGet: creature => { creature.Spawn(spawnPosition); },
            actionOnRelease: creature=> creature.gameObject.SetActive(false),
            actionOnDestroy: creature => Destroy(creature.gameObject),
            collectionCheck: false,
            defaultCapacity: creatureMaxCapactiy,
            maxSize: creatureMaxCapactiy);
    }

    public Creature Get()
    {
        var creature = creaturePool.Get();
        creatureActiveMap.Add(creature.creatureIndex, creature);
        return creature;
    }

    public void Release(Creature creature)
    {
        creaturePool.Release(creature);
        creatureActiveMap.Remove(creature.creatureIndex);
    }

    public void Update()
    {
        //Iterate over the Creatures so we don't have to have up to 240 extra updates running
        foreach (var creature in creatureActiveMap)
        {
            creature.Value.Tick();
        }
    }

}
