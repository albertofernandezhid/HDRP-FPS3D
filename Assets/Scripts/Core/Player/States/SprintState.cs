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

        if (player.staminaSystem != null)
            player.staminaSystem.Consume(Time.deltaTime, player.staminaSystem.sprintDrainMultiplier);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();
        if (!player.IsGrounded()) return this;
        if (moveInput.magnitude < 0.1f) return new IdleState(player);

        if (player.staminaSystem != null && !player.staminaSystem.HasStamina())
        {
            player.SetSprintBlocked(true);
            return new WalkState(player);
        }

        bool wantsSprint = player.IsSprintPressed() && !player.IsSprintBlocked();
        bool wantsRun = player.IsRunPressed() && !player.IsRunBlocked();

        if (wantsSprint && wantsRun)
        {
            if (player.GetLastRunTime() > player.GetLastSprintTime())
                return new RunState(player);
        }
        else if (!wantsSprint)
        {
            if (wantsRun)
                return new RunState(player);
            else
                return new WalkState(player);
        }

        return this;
    }

    public override float GetSpeed() => player.sprintSpeed;
    public override void Exit() { }
}