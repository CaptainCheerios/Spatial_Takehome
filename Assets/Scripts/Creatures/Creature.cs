using UnityEngine;

public class Creature : MonoBehaviour
{
    public int creatureIndex = -1;

    public Rigidbody Rigidbody;
    [SerializeField] private float spawnImpulse;

    public ICreatureState currentState = new IdleState();
    public void Spawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.AddForce(Random.onUnitSphere * spawnImpulse, ForceMode.Impulse);
        currentState = new IdleState();
        CaptureGameMode.Instance.RegisterCreature();
    }

    public void Captured()
    {

        CaptureGameMode.Instance.Captured();
        CreatureManager.Instance.Release(this);
    }

    public void Tick()
    {
        currentState.Tick(this);
    }
}
