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
            velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        }

        player.Move(velocity);
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