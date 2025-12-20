using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;

    public PlayerState(PlayerController playerController)
    {
        player = playerController;
    }

    public abstract void Enter();
    public abstract void HandleMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested);
    public abstract PlayerState UpdateState();
    public abstract float GetSpeed();
    public abstract void Exit();

    protected void BaseMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested, float speedMultiplier = 1f)
    {
        float currentSpeed = GetSpeed() * speedMultiplier;
        Vector3 moveDirection = player.GetCameraRelativeDirection(input);

        player.ApplyGravity(ref velocity);

        if (jumpRequested && player.IsGrounded())
        {
            float effectiveJumpHeight = player.jumpHeight * player.jumpHeightMultiplier;
            velocity.y = Mathf.Sqrt(effectiveJumpHeight * -2f * player.gravity);
        }

        Vector3 horizontalMotion = moveDirection * currentSpeed;
        Vector3 motion = new Vector3(horizontalMotion.x, velocity.y, horizontalMotion.z);
        player.Move(motion);
    }
}