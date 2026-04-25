using UnityEngine;

public class Grabber : MonoBehaviour
{
    public LayerMask grabberLayerMask;
    public GrabbableObject grabbableObject;

    public float launchForce = 100f;
    public float springStrength = 500f;
    public float damping = 20f;

    public void Grab()
    {
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, grabberLayerMask);
        if (hit.collider == null) return;

        var grabbed = hit.collider.gameObject.GetComponent<GrabbableObject>();
        if (grabbed == null) return;

        grabbableObject = grabbed;
    }

    private void FixedUpdate()
    {
        if (grabbableObject == null) return;

        Vector3 displacement = transform.position - grabbableObject.transform.position;
        Vector3 springForce = displacement * springStrength;
        Vector3 dampForce = -grabbableObject.rb.linearVelocity * damping;

        grabbableObject.rb.AddForce(springForce + dampForce);
    }

    public void LaunchGrabbedObject()
    {
        if (grabbableObject == null) return;

        grabbableObject.rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        grabbableObject = null;
    }
}