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

        if (player.staminaSystem != null)
            player.staminaSystem.Consume(Time.deltaTime, player.staminaSystem.runDrainMultiplier);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();
        if (!player.IsGrounded()) return this;
        if (moveInput.magnitude < 0.1f) return new IdleState(player);

        if (player.staminaSystem != null && !player.staminaSystem.HasStamina())
        {
            player.SetRunBlocked(true);
            return new WalkState(player);
        }

        bool wantsSprint = player.IsSprintPressed() && !player.IsSprintBlocked();
        bool wantsRun = player.IsRunPressed() && !player.IsRunBlocked();

        if (wantsSprint && wantsRun)
        {
            if (player.GetLastSprintTime() > player.GetLastRunTime())
                return new SprintState(player);
        }
        else if (wantsSprint)
        {
            return new SprintState(player);
        }

        if (!player.IsRunPressed())
            return new WalkState(player);

        return this;
    }

    public override float GetSpeed() => player.runSpeed;
    public override void Exit() { }
}