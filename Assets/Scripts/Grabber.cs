using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grabber : MonoBehaviour
{
    [NonSerialized]
    public GrabbableObject targetedObject;

    public float launchForce = 10f;

    public bool isHolding = false;
    
    [Header("Leash")]
    public float springStrength = 500f;
    public float damping = 20f;
    public float breakDistance = 6f;
    
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
    }
    
    
    private void DropGrabbedObject()
    {
        isHolding = false;
    }

    private void FixedUpdate()
    {
        if (!isHolding) return;
        if (targetedObject.isBroken)
        {
            DropGrabbedObject();
            return;
        }

        Vector3 displacement = transform.position - targetedObject.transform.position;

        // Sanity leash: object stuck behind a wall, etc.
        if (displacement.sqrMagnitude > breakDistance * breakDistance)
        {
            DropGrabbedObject();
            return;
        }

        Vector3 springForce = displacement * springStrength;
        Vector3 dampForce = -targetedObject.rigidBody.linearVelocity * damping;
        targetedObject.rigidBody.AddForce(springForce + dampForce);
    }

    private void Update()
    {
        if (isHolding)
            return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, grabberLayerMask))
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
        }
    }

    public void LaunchGrabbedObject()
    {
        if(!isHolding)
            return;
        targetedObject.rigidBody.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        targetedObject = null;
    }
}