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

    // ?? Método común para movimiento básico
    protected void BaseMovement(Vector2 input, ref Vector3 velocity, bool jumpRequested, float speedMultiplier = 1f)
    {
        float currentSpeed = GetSpeed() * speedMultiplier;
        Vector3 moveDirection = player.GetCameraRelativeDirection(input);

        // Aplicar gravedad
        player.ApplyGravity(ref velocity);

        // Manejar salto
        if (jumpRequested && player.IsGrounded())
        {
            velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        }

        // Movimiento horizontal
        Vector3 horizontalMotion = moveDirection * currentSpeed;

        // Movimiento final
        Vector3 motion = new Vector3(horizontalMotion.x, velocity.y, horizontalMotion.z);
        player.Move(motion);

        // Rotación (usando el método sin parámetros)
        player.RotateTowardsMovement();
    }
}