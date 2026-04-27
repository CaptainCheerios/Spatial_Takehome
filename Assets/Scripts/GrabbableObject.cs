using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour
{
    [Header("Physics")]
    public Rigidbody rigidBody;
    
    [Header("Breaking Stuff")]
    public float breakForce = 1f;
    [NonSerialized] public bool isBroken = false;
    [Range(0,8)] public int maxCreatureCount = 8;
    public float critterSpawnRadius = 0.1f;
    
    [Header("Visuals/FX")]
    public GameObject breakVFXPrefab;
    public Material brokenMaterial;
    
    public AudioClip breakSFX;
    public AudioClip[] impactSFX;
    [SerializeField]
    private float impactSoundDelay = 0.2f;

    private float lastImpactSoundTime = 0;

    [SerializeField] private MeshRenderer[] meshRenderers;

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            //So I don't have to deal with assigning these
            List<MeshRenderer> meshRenderersFound = GetComponents<MeshRenderer>().ToList();
            meshRenderersFound.AddRange(GetComponentsInChildren<MeshRenderer>());
            if (meshRenderersFound.Count == 0)
            {
                Debug.LogWarning($"{this.name} was unable to find any mesh renderers");
            }
            else
            {
                meshRenderers = meshRenderersFound.ToArray();
                Debug.Log($"Assigning Mesh Renderers to {this.name}");
            }

            if (rigidBody == null)
            {
                rigidBody = GetComponent<Rigidbody>();
                if (rigidBody == null)
                    Debug.LogWarning($"{this.name} is missing a rigidbody component");
            }


            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.renderingLayerMask = 1;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
            return;

        float impact = collision.relativeVelocity.magnitude * rigidBody.mass;

        if (Time.time - lastImpactSoundTime > impactSoundDelay && impactSFX.Length>0)
        {
            var audioClip = impactSFX[Random.Range(0, impactSFX.Length)];
            AudioSource.PlayClipAtPoint(audioClip, this.transform.position);
        }

        if (impact >= breakForce)
        {
            Break();
        }
        
    }

    private void Break()
    {
        if (isBroken)
            return;
        isBroken = true;

        gameObject.layer = LayerMask.NameToLayer("BrokenPieces");

        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("BrokenPieces");
        }

        //Change the material
        if (brokenMaterial)
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.sharedMaterial = brokenMaterial;
            }
        }
        //Play SFX
        if(breakSFX)
            AudioSource.PlayClipAtPoint(breakSFX, this.transform.position);
        if (breakVFXPrefab)
             Instantiate(breakVFXPrefab, this.transform.position, Quaternion.identity);
        SetHighlight(false);
        CreatureAndSpawnerCounter.Instance.GrabbableBroken();
        //Spawn Creatures
        for (int i = 0; i < maxCreatureCount; i++)
        {
            CreatureManager.Instance.Get().Spawn(transform.position + GetRandomSpawnOffset());
        }
    }

    private Vector3 GetRandomSpawnOffset()
    {
        return Random.insideUnitSphere * critterSpawnRadius;
    }

    public void SetHighlight(bool enable)
    {
        foreach (var meshRenderer in meshRenderers)
        {
            uint outlineLayer = (uint)1 << 8;
            meshRenderer.renderingLayerMask = enable
                ? meshRenderer.renderingLayerMask | outlineLayer
                : meshRenderer.renderingLayerMask & ~outlineLayer;
 
        }
    }
}
