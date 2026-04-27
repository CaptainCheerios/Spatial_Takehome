using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WorldBoundsVolume : MonoBehaviour
{
    [SerializeField] private float clearanceRadius = 0.35f;
    [SerializeField] private LayerMask spawnBlockers;

    private void OnTriggerExit(Collider other)
    {
        var creature = other.GetComponent<Creature>();
        if (!creature) return;

        Vector3 safe = SpawnPositionResolver.Resolve(transform.position, clearanceRadius, spawnBlockers, out _);
        creature.transform.position = safe;
    }

    void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider>();
        if (!box) return;
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.10f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}