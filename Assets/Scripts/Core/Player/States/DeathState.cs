using UnityEngine;

public class DeathState : PlayerState
{
    public DeathState(PlayerController playerController) : base(playerController) { }

    public override void Enter()
    {
        player.enabled = false;
    }

    public override void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested)
    {
        player.ApplyGravity(ref velocity);
        player.Move(velocity);
    }

    public override PlayerState UpdateState()
    {
        return this;
    }

    public override float GetSpeed() => 0f;

    public override void Exit()
    {
        player.enabled = true;
    }
}