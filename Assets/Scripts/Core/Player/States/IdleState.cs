using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController playerController) : base(playerController) { }

    public override void Enter()
    {
        if (player.animationController != null)
            player.animationController.SetIdle();
    }

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
            if (player.IsSprintPressed() && player.staminaSystem != null && player.staminaSystem.HasStamina())
                return new SprintState(player);
            else if (player.IsRunPressed())
                return new RunState(player);
            else
                return new WalkState(player);
        }

        return this;
    }

    public override float GetSpeed() => 0f;
    public override void Exit() { }
}