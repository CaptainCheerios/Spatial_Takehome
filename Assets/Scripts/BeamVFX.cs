using System;
using UnityEngine;

public class BeamVFX : MonoBehaviour
{
    public LineRenderer line;
    public int pointCount = 4;
    private float bowFraction = 0.15f;

    private void Awake()
    {
        line.useWorldSpace = true;
        line.positionCount = pointCount;
        line.enabled = false;

    }

    public void DrawBeam(Vector3 endPoint)
    {
        if (line.positionCount != pointCount) line.positionCount = pointCount;
        if (!line.enabled) line.enabled = true;

        Vector3 start = transform.position;
        Vector3 delta = endPoint - start;
        float distance = delta.magnitude;

        // Degenerate case: object is on top of the start anchor.
        if (distance < 0.001f)
        {
            for (int i = 0; i < pointCount; i++) line.SetPosition(i, start);
            return;
        }

        Vector3 dir = delta / distance;

        // Perpendicular direction: project this transform's up onto the plane perpendicular to dir.
        // Using transform.up means the bow follows the player's orientation (always bows "above" relative to the camera).
        Vector3 reference = transform.up;
        Vector3 perp = reference - Vector3.Project(reference, dir);

        // Fallback if reference is (nearly) parallel to dir — pick a different axis.
        if (perp.sqrMagnitude < 0.0001f)
        {
            Vector3 fallback = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
            perp = fallback - Vector3.Project(fallback, dir);
        }
        perp.Normalize();

        // Single quadratic bezier control point at midpoint + perpendicular offset.
        // Factor of 2: a quadratic bezier at t=0.5 only reaches halfway to the control point's offset,
        // so doubling here means bowFraction is the actual peak displacement as a fraction of length.
        Vector3 control = Vector3.Lerp(start, endPoint, 0.5f) + perp * (distance * bowFraction * 2f);

        // Sample the bezier at evenly spaced t values.
        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            float omt = 1f - t;
            Vector3 p = (omt * omt) * start
                        + (2f * omt * t) * control
                        + (t * t) * endPoint;
            line.SetPosition(i, p);
        }
    }

    public void Hide()
    {
        if (line.enabled) line.enabled = false;
    }
}
