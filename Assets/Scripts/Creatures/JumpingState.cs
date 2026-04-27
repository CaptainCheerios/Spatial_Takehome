using UnityEngine;

public class JumpingState : ICreatureState
{
    public float jumpDelay = 4f;
    private float lastJumpTime = 0;
    public void Tick(Creature self)
    {
        if (Time.time - lastJumpTime < jumpDelay)
            return;
        lastJumpTime = Time.time;
        self.Jump(UnityEngine.Random.onUnitSphere);
    }

    public void EnteredState(Creature self)
    {
        lastJumpTime = Time.time - jumpDelay * self.PhaseOffset;
    }
}