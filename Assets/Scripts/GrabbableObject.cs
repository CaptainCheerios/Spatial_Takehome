using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody rigidBody;

    public float breakForce = 1f;
    public int maxCreatureCount = 8;
    public GameObject breakVFXPrefab;
    public AudioClip breakSFX;
    public AudioClip[] impactSFX;

    [NonSerialized] public bool isBroken;

    [SerializeField] private MeshRenderer[] meshRenderers; 
    
    private void OnValidate()
    {
        //So I don't have to deal with assigning these
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length == 0)
        {
            Debug.LogWarning($"{this.name} was unable to find any mesh renderers" );
        }

        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
            if (rigidBody == null)
                Debug.LogWarning($"{this.name} is missing a rigidbody component");
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
            return;

        float impact = collision.relativeVelocity.magnitude * rigidBody.mass;

        var audioClip = impactSFX[Random.Range(0, impactSFX.Length)];
        
    }

    private void Break()
    {
        
    }
}
