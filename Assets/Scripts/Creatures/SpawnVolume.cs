using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnVolume : MonoBehaviour
{
    public static readonly List<SpawnVolume> All = new();

    public BoxCollider Box { get; private set; }

    void Awake()
    {
        Box = GetComponent<BoxCollider>();
        Box.isTrigger = true; // we never want it to push physics
    }

    void OnEnable()  => All.Add(this);
    void OnDisable() => All.Remove(this);

    public bool Contains(Vector3 worldPos) => Box.bounds.Contains(worldPos);

    public Vector3 RandomPointInside()
    {
        Vector3 e = Box.size * 0.5f;
        Vector3 local = new(
            Random.Range(-e.x, e.x),
            Random.Range(-e.y, e.y),
            Random.Range(-e.z, e.z));
        return transform.TransformPoint(Box.center + local);
    }

    public Vector3 Center => transform.TransformPoint(Box.center);

    void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider>();
        if (!box) return;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}