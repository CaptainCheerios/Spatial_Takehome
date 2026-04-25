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
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, grabberLayerMask);
        if (hit.collider == null) return;

        var grabbed = hit.collider.gameObject.GetComponent<GrabbableObject>();
        if (grabbed == null) return;

        targetedObject = grabbed;
    }

    private void FixedUpdate()
    {
        if (!isHolding) return;

        Vector3 displacement = transform.position - targetedObject.transform.position;
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
                if (targetedObject && grabbableObject != targetedObject)
                {
                    //grabbableObject.ToggleHighlight(false);
                    targetedObject = grabbableObject;
                        //grabbableObject.ToggleHighlight(true);
                }
            }
        }
    }

    private void DropGrabbedObject()
    {
        
    }

    public void LaunchGrabbedObject()
    {
        if (targetedObject == null) return;

        targetedObject.rigidBody.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        targetedObject = null;
    }
}