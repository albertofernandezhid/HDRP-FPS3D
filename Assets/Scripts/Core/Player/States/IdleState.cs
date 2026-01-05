using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController playerController) : base(playerController) { }

    public override void Enter() { }

    public override void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested)
    {
        player.ApplyGravity(ref velocity);

        if (jumpRequested && player.IsGrounded())
        {
            float effectiveJumpHeight = player.jumpHeight * player.jumpHeightMultiplier;
            velocity.y = Mathf.Sqrt(effectiveJumpHeight * -2f * player.gravity);
        }

        player.Move(velocity);

        if (player.staminaSystem != null)
            player.staminaSystem.Regenerate(Time.deltaTime, player.staminaSystem.idleRegenMultiplier);
    }

    public override PlayerState UpdateState()
    {
        Vector2 moveInput = player.GetMoveInput();
        if (!player.IsGrounded()) return this;

        if (moveInput.magnitude > 0.1f)
        {
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

            return new WalkState(player);
        }

        return this;
    }

    public override float GetSpeed() => 0f;
    public override void Exit() { }
}