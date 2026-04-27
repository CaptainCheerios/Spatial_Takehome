using UnityEngine;

public static class SpawnPositionResolver
{
    const int   RingCount          = 6;     // how many radii to try
    const float RingStep           = 0.25f; // meters between rings
    const int   SamplesPerRing     = 8;     // directions sampled at each radius
    static readonly Collider[] overlapBuffer = new Collider[1];

    /// <summary>
    /// Returns a position as close to <paramref name="desired"/> as possible that is
    /// inside a SpawnVolume and not penetrating <paramref name="blockers"/>.
    /// </summary>
    public static Vector3 Resolve(Vector3 desired, float clearanceRadius, LayerMask blockers)
    {
        if (SpawnVolume.All.Count == 0) return desired;

        // Pick the volume that contains 'desired', else clamp into the nearest one.
        SpawnVolume volume = null;
        foreach (var v in SpawnVolume.All)
            if (v.Contains(desired)) { volume = v; break; }

        if (volume == null)
        {
            float best = float.MaxValue;
            foreach (var v in SpawnVolume.All)
            {
                float d = (v.Center - desired).sqrMagnitude;
                if (d < best) { best = d; volume = v; }
            }
            desired = volume.Box.bounds.ClosestPoint(desired);
        }

        // 1) The exact requested spot.
        if (IsValid(desired, clearanceRadius, blockers, volume)) return desired;

        // 2) Expanding rings around the requested spot.
        for (int ring = 1; ring <= RingCount; ring++)
        {
            float radius = ring * RingStep;
            for (int i = 0; i < SamplesPerRing; i++)
            {
                // Fibonacci-ish spread on a sphere — even directional coverage.
                float t = (i + 0.5f) / SamplesPerRing;
                float phi = Mathf.Acos(1f - 2f * t);
                float theta = Mathf.PI * (1f + Mathf.Sqrt(5f)) * i;
                Vector3 dir = new(
                    Mathf.Sin(phi) * Mathf.Cos(theta),
                    Mathf.Cos(phi),
                    Mathf.Sin(phi) * Mathf.Sin(theta));

                Vector3 candidate = desired + dir * radius;
                if (IsValid(candidate, clearanceRadius, blockers, volume))
                    return candidate;
            }
        }

        // 3) Last resort — volume center.
        return volume.Center;
    }

    static bool IsValid(Vector3 p, float r, LayerMask mask, SpawnVolume volume)
    {
        if (!volume.Contains(p)) return false;
        return Physics.OverlapSphereNonAlloc(p, r, overlapBuffer, mask, QueryTriggerInteraction.Ignore) == 0;
    }
}