using UnityEngine;

public class WalkState : PlayerState
{
    public WalkState(PlayerController playerController) : base(playerController) { }

    public override void Enter() { }

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

        bool wantsRun = player.IsRunPressed() && !player.IsRunBlocked();
        bool wantsSprint = player.IsSprintPressed() && !player.IsSprintBlocked();

        if (player.staminaSystem != null && player.staminaSystem.CanEnterStaminaState())
        {
            if (wantsSprint && wantsRun)
            {
                if (player.GetLastSprintTime() > player.GetLastRunTime())
                    return new SprintState(player);
                else
                    return new RunState(player);
            }
            else if (wantsSprint)
            {
                return new SprintState(player);
            }
            else if (wantsRun)
            {
                return new RunState(player);
            }
        }

        return this;
    }

    public override float GetSpeed() => player.walkSpeed;
    public override void Exit() { }
}