using UnityEngine;

public class SprintState : PlayerState
{
    public SprintState(PlayerController playerController) : base(playerController) { }

    public override void Enter()
    {
        if (player.animationController != null)
            player.animationController.SetSprinting();
    }

    public override void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested)
    {
        BaseMovement(input, ref velocity, jumpRequested);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();

        if (!player.IsGrounded()) return this;

        if (moveInput.magnitude < 0.1f) return new IdleState(player);

        if (player.staminaSystem != null && !player.staminaSystem.HasStamina())
        {
            if (player.IsRunPressed()) return new RunState(player);
            else return new WalkState(player);
        }

        if (!player.IsSprintPressed())
        {
            if (player.IsRunPressed()) return new RunState(player);
            else return new WalkState(player);
        }

        return this;
    }

    public override float GetSpeed() => player.sprintSpeed;

    public override void Exit() { }
}