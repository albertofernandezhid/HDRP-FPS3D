using UnityEngine;

public class WalkState : PlayerState
{
    public WalkState(PlayerController playerController) : base(playerController) { }

    public override void Enter()
    {
        if (player.animationController != null)
            player.animationController.SetWalking();
    }

    public override void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested)
    {
        BaseMovement(input, ref velocity, jumpRequested);

        if (player.staminaSystem != null)
            player.staminaSystem.Regenerate(Time.deltaTime, player.staminaSystem.walkRegenMultiplier);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();
        if (!player.IsGrounded()) return this;
        if (moveInput.magnitude < 0.1f) return new IdleState(player);

        if (player.IsSprintPressed() && player.staminaSystem != null && player.staminaSystem.HasStamina())
            return new SprintState(player);
        else if (player.IsRunPressed())
            return new RunState(player);

        return this;
    }

    public override float GetSpeed() => player.walkSpeed;
    public override void Exit() { }
}