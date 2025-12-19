using UnityEngine;

public class RunState : PlayerState
{
    public RunState(PlayerController playerController) : base(playerController) { }

    public override void Enter()
    {
        if (player.animationController != null)
            player.animationController.SetRunning();
    }

    public override void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested)
    {
        BaseMovement(input, ref velocity, jumpRequested);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();

        if (!player.IsGrounded())
        {
            return this;
        }

        if (moveInput.magnitude < 0.1f)
        {
            return new IdleState(player);
        }

        if (player.IsSprintPressed() && player.staminaSystem != null && player.staminaSystem.HasStamina())
        {
            return new SprintState(player);
        }
        else if (!player.IsRunPressed())
        {
            return new WalkState(player);
        }

        return this;
    }

    public override float GetSpeed() => player.runSpeed;

    public override void Exit()
    {
        
    }
}