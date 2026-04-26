using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vacuum : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private SphereCollider triggerSphere;

    [SerializeField] private Transform vacuumNozzle;
    [SerializeField] private Transform vacuumAngleCheck;
    [SerializeField] private Grabber grabber;
    [SerializeField] private AudioSource vacuumSound;

    [Header("Settings")]
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private float captureDistance = 0.05f;
    [SerializeField] private float maxAngle = 15f;
    [SerializeField] private float vacuumPullForce = 10f;

    [Header("FX")] private AudioClip vacuumEffect;

    [Header("Debug")]
    [SerializeField] private int debugRayCount = 16;
    [SerializeField] private Color debugConeColor = Color.cyan;

    [NonSerialized] public bool isActive = false;

    //Set the capacity to 80 so we hopefully don't have to resize.
    private HashSet<Creature> trackedCreatures = new();

    private void OnValidate()
    {
        if (triggerSphere)
            triggerSphere.radius = maxRange;

        if (!grabber)
            Debug.LogError("Vacuum is missing a grabber component assignment");
    }

    public void VacuumInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Activate();
        }
        else if (context.canceled)
        {
            Deactivate();
        }
    }

    public void Activate()
    {
        if (grabber.isHolding)
            grabber.DropGrabbedObject();
        isActive = true;
        SetFX(true);
    }

    public void Deactivate()
    {
        if (isActive)
        {
            isActive = false;
            SetFX(false);
        }
    }

    private void SetFX(bool state)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        var creature = other.GetComponent<Creature>();
        if (creature)
            trackedCreatures.Add(creature);
    }

    private void OnTriggerExit(Collider other)
    {
        var creature = other.GetComponent<Creature>();
        if (creature)
            trackedCreatures.Remove(creature);

    }

    private void FixedUpdate()
    {
        if (!isActive || trackedCreatures.Count == 0)
            return;
        var snapshot = new List<Creature>(trackedCreatures);
        foreach (var creature in snapshot)
        {
            if (!creature.gameObject.activeInHierarchy)
            {
                trackedCreatures.Remove(creature);
                continue;
            }

            Vector3 toCreature = creature.transform.position - vacuumNozzle.transform.position;
            float distance = toCreature.magnitude;
            //Check if it's in range
            if(distance> maxRange)
                continue;
            //Check if the creature is within our vacuum's angle.
            //Using a second transform so we don't have to worry about objects missing the nozzle when they get close
            float angle = Vector3.Angle(vacuumAngleCheck.forward, toCreature.normalized);
            if(angle > maxAngle)
                continue;
            
            //Pull the creature in and increase based on closeness
            float closeness = 1f - (distance / maxRange);
            creature.Rigidbody.AddForce(-toCreature.normalized * (vacuumPullForce * (0.3f + closeness)));

            if (distance < captureDistance)
            {
                trackedCreatures.Remove(creature);
                creature.Captured();
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (vacuumAngleCheck)
            DrawDebugCone(debugRayCount, debugConeColor);
    }

    private void DrawDebugCone(int rayCount, Color color)
    {
        Vector3 origin = vacuumAngleCheck.position;
        Vector3 forward = vacuumAngleCheck.forward;

        Gizmos.color = color;

        // Center ray along the vacuum axis
        Gizmos.DrawRay(origin, forward * maxRange);

        // Perimeter rays distributed evenly around the cone edge
        for (int i = 0; i < rayCount; i++)
        {
            float azimuth = 360f / rayCount * i;
            Vector3 perp = Quaternion.AngleAxis(azimuth, forward) * vacuumAngleCheck.right;
            Vector3 dir = Quaternion.AngleAxis(maxAngle, perp) * forward;
            Gizmos.DrawRay(origin, dir * maxRange);
        }
    }
}
