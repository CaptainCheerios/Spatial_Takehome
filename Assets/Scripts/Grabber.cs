using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    [NonSerialized]
    private GrabbableObject targetedObject;

    public GrabbableObject grabbedObject;

    [SerializeField] private Transform raycastTransform;

    public float launchForce = 10f;

    public bool isHolding = false;
    
    [Header("Leash")]
    public float springStrength = 500f;
    public float damping = 20f;
    public float breakDistance = 6f;
    public BeamVFX beam;
    public Transform grabberTransform;
    
    [Header("Grabber RayCast")]
    public LayerMask grabberLayerMask;
    public float maxDistance;

    public void Grab(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            DropGrabbedObject();
        }
        else
        {
            GrabObject();
        }
    }

    private void GrabObject()
    {
        if (targetedObject == null)
            return;
        isHolding = true;
        grabbedObject = targetedObject;
    }
    
    
    public void DropGrabbedObject()
    {
        isHolding = false;
        grabbedObject = null;
        beam.Hide();
    }

    private void FixedUpdate()
    {
        if (!isHolding || !grabbedObject) return;
        if (grabbedObject.isBroken)
        {
            DropGrabbedObject();
            return;
        }

        Vector3 displacement = grabberTransform.position - grabbedObject.transform.position;

        // Sanity leash: object stuck behind a wall, etc.
        if (displacement.sqrMagnitude > breakDistance * breakDistance)
        {
            DropGrabbedObject();
            return;
        }

        Vector3 springForce = displacement * springStrength;
        Vector3 dampForce = -grabbedObject.rigidBody.linearVelocity * damping;
        grabbedObject.rigidBody.AddForce(springForce + dampForce);
        beam.DrawBeam(grabbedObject.transform.position);
    }

    private void Update()
    {
        if (isHolding)
            return;

        if (Physics.Raycast(raycastTransform.position, raycastTransform.forward, out RaycastHit hit, grabberLayerMask))
        {
            var grabbableObject = hit.collider.gameObject.GetComponent<GrabbableObject>();
            if (grabbableObject)
            {
                if (grabbableObject != targetedObject)
                {
                    if(targetedObject)
                        targetedObject.SetHighlight(false);
                    targetedObject = grabbableObject;
                        grabbableObject.SetHighlight(true);
                }
            }
            else
            {
                if(targetedObject)
                    targetedObject.SetHighlight(false);
                targetedObject = null;
            }
        }
    }

    public void LaunchGrabbedObject(InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        if(!isHolding)
            return;
        grabbedObject.rigidBody.AddForce(grabberTransform.forward * launchForce, ForceMode.Impulse);
        grabbedObject = null;
    }
}