using Unity.VisualScripting;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public int creatureIndex = -1;

    public Rigidbody Rigidbody;
    [SerializeField] private float spawnImpulse;
    [SerializeField] private AudioClip[] capturedSFX;

    public ICreatureState currentState = new IdleState();
    public void Spawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.AddForce(Random.onUnitSphere * spawnImpulse, ForceMode.Impulse);
        currentState = new IdleState();
        // CaptureGameMode.Instance.RegisterCreature();
    }

    public void Captured()
    {
        var audioClip = capturedSFX[Random.Range(0, capturedSFX.Length)];
        AudioSource.PlayClipAtPoint(audioClip, this.transform.position);
        CaptureGameMode.Instance.Captured();
        CreatureManager.Instance.Release(this);
        Debug.Log($"Captured creature {creatureIndex}");
    }

    public void Tick()
    {
        currentState.Tick(this);
    }
}
