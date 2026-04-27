using Unity.VisualScripting;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public int creatureIndex = -1;

    public Rigidbody Rigidbody;
    [SerializeField] private float spawnImpulse;
    [SerializeField] private AudioClip[] capturedSFX;

    [SerializeField] private float spawnClearanceRadius = 0.25f;
    [SerializeField] private LayerMask spawnBlockers;

    public float jumpForce = 1f;

    public enum BehaviorState
    {
        Jumping,
        Idle
    }

    public BehaviorState CurrentBehaviorState { get; private set; } = BehaviorState.Idle;

    public float PhaseOffset { get; set; } = 0;
    public ICreatureState currentState = new IdleState();
    
    public void Spawn(Vector3 position)
    {
        position = SpawnPositionResolver.Resolve(position, spawnClearanceRadius, spawnBlockers, out Vector3 awayHint);

        transform.position = position;
        gameObject.SetActive(true);
        Rigidbody.linearVelocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;

        // Bias the spawn impulse away from whatever the resolver had to dodge.
        // If a random direction points back into the obstacle, reflect it across
        // the away-direction so it launches into open air instead.
        Vector3 dir = Random.onUnitSphere;
        if (awayHint != Vector3.zero && Vector3.Dot(dir, awayHint) < 0f)
            dir = Vector3.Reflect(dir, awayHint);

        Rigidbody.AddForce(dir * spawnImpulse, ForceMode.Impulse);
        SwitchToState(BehaviorState.Jumping);
        CreatureAndSpawnerCounter.Instance.RegisterCreature();
    }
    public void Jump(Vector3 direction)
    {
        Rigidbody.AddForce(direction*jumpForce, ForceMode.Impulse);
    }

    public void Captured()
    {
        var audioClip = capturedSFX[Random.Range(0, capturedSFX.Length)];
        AudioSource.PlayClipAtPoint(audioClip, this.transform.position);
        CreatureAndSpawnerCounter.Instance.Captured();
        CreatureManager.Instance.Release(this);
    }

    public void Tick()
    {
        currentState.Tick(this);
    }

    public void SwitchToState(BehaviorState state)
    {
        if (CurrentBehaviorState == state)
            return;
        switch (state)
        {
            case BehaviorState.Idle:
                currentState = new IdleState();
                break;
            case BehaviorState.Jumping:
                currentState = new JumpingState();
                break;
        }
        currentState.EnteredState(this);
    }
}
