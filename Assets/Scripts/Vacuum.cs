using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    [SerializeField] private float breakConnectionAngle = 45f;
    
    [Header("Settings/Vacuum Forces")]
    //How fast it pulls
    [SerializeField] private float axialPullSpeed = 8f;
    //How fast it pulls towards that center line
    [SerializeField] private float radialPullSpeed = 12f;
    //Dampens the oscillations when it pulls towards the center of the vacuum path.
    [SerializeField] private float radialStiffness = 20f;
    [SerializeField] private float pullAcceleration = 60f;

    [Header("FX")] private AudioClip vacuumEffect;

    [Header("Debug")]
    [SerializeField] private int debugRayCount = 16;
    [SerializeField] private Color debugConeColor = Color.cyan;

    [NonSerialized] public bool isActive = false;

    //Set the capacity to 80 so we hopefully don't have to resize.
    private HashSet<Creature> trackedCreatures = new();
    private HashSet<Creature> pullingCreatures = new();

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
            pullingCreatures.Clear();
            SetFX(false);
        }
    }

    private void SetFX(bool state)
    {
        if (state)
        {
            vacuumSound.Play();
        }
        else
        {
            vacuumSound.Stop();
        }
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
        {
            pullingCreatures.Remove(creature);
            trackedCreatures.Remove(creature);
        }

    }

    private void FixedUpdate()
    {
        if (!isActive || trackedCreatures.Count == 0)
            return;

        Vector3 nozzlePos = vacuumNozzle.position;
        Vector3 axisDir = vacuumAngleCheck.forward;
        float dt = Time.fixedDeltaTime;

        var snapshot = new List<Creature>(trackedCreatures);
        foreach (var creature in snapshot)
        {
            if (!creature.gameObject.activeInHierarchy)
            {
                trackedCreatures.Remove(creature);
                continue;
            }

            Vector3 toCreature = creature.transform.position - nozzlePos;
            float distance = toCreature.magnitude;
            if (distance > maxRange) 
                continue;

            float angle = Vector3.Angle(axisDir, toCreature.normalized);
            
            //If a creature is in the cone we are pulling it in and not letting it go.
            bool inCone = angle <= maxAngle;
            if (!inCone && !pullingCreatures.Contains(creature))
                continue;
            pullingCreatures.Add(creature);
            
            float axialOffset = Vector3.Dot(toCreature, axisDir);
            Vector3 radialVec = toCreature - axisDir * axialOffset;
            float radialDist = radialVec.magnitude;
            Vector3 radialDir = radialDist > 1e-4f ? radialVec / radialDist : Vector3.zero;

            
            float closeness = 1f - Mathf.Clamp01(axialOffset / maxRange);
            float radialTargetSpeed = Mathf.Min(radialDist * radialStiffness, radialPullSpeed);
            Vector3 targetVel = -axisDir  * (axialPullSpeed * (0.3f + closeness)) - radialDir *  radialTargetSpeed;
            
            var rigidbody = creature.Rigidbody;
            rigidbody.linearVelocity = Vector3.MoveTowards(rigidbody.linearVelocity, targetVel, pullAcceleration * dt);

            if (distance < captureDistance)
            {
                trackedCreatures.Remove(creature);
                pullingCreatures.Remove(creature);
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
