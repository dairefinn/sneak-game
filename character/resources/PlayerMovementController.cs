namespace SneakGame;

using Godot;

[GlobalClass]
public partial class PlayerMovementController : Resource
{

	[ExportGroup("Physics")]
    [Export] public int SPEED = 10;
	[Export] public int DASH_MULTIPLIER = 2;
    [Export] public int JUMP_HEIGHT = 5;
    [Export] public int CROUCH_HEIGHT = 2;
	[Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 GRAVITY_VECTOR = new(0, -9.8f, 0); // TODO: Replace with actual gravity value when I figure out where to put it

    public void OnProcess(Player player, double delta)
    {
        if(player == null) return;

		CameraController Camera = CameraController.Instance;
		
		Vector3 desiredMovement = GetDesiredMovementVector();
		if (Camera != null)
		{
			if (Camera.FocusedEntity == null)
			{
				Camera.FocusedEntity = player;
			}

			// Movement is based on the direction of the camera.
			// Eg - holding `move_left` will move towards the left of the camera and not the world origin
			desiredMovement = desiredMovement.Rotated(Vector3.Up, Camera.Rotation.Y);

			Vector3 newRotation = player.Rotation;
			newRotation.Y = Camera.Rotation.Y + Mathf.Pi;
			player.Rotation = newRotation;
		}
		
		ApplyPhysics(player, desiredMovement, delta);

		player.MoveAndSlide();
    }


	private void ApplyPhysics(Player player, Vector3 desiredMovement, double delta)
	{
		Vector3 newVelocity = player.Velocity;

		newVelocity = ApplyGravity(player, newVelocity, delta);
		newVelocity = ApplyMovement(player, desiredMovement, newVelocity, delta);

		player.Velocity = newVelocity;
	}

	private Vector3 ApplyGravity(Player player, Vector3 velocity, double delta)
	{
		if (!AffectedByGravity) return velocity;

		if (player.IsOnFloor())
		{
			velocity.Y = 0;
		}
		else
		{
			velocity.Y += (float)(GRAVITY_VECTOR.Y * delta);
		}

		return velocity;
	}

	public Vector3 ApplyMovement(Player player, Vector3 desiredMovement, Vector3 velocity, double delta)
	{
		Vector3 movement = desiredMovement * SPEED;

		velocity.X = movement.X;
		velocity.Z = movement.Z;

		// NOTE: If slide is applied after the jump, it goes extra high. It might be a cool mechanic to have.
		velocity = CrouchOrSlide(player, velocity, delta);

        if (Input.IsActionPressed("move_jump") && player.IsOnFloor())
        {
            velocity = Jump(player, velocity, delta);
        }

		return velocity;
	}

	public Vector3 Jump(Player player, Vector3 velocity, double delta)
	{
		if (player.IsOnFloor())
		{
			velocity.Y = JUMP_HEIGHT;
		}

		return velocity;
	}

	public Vector3 CrouchOrSlide(Player player, Vector3 velocity, double delta)
	{
		Vector3 newScale = player.Hitbox.Scale;

        if (Input.IsActionPressed("move_crouch")) // && IsOnFloor() <-- Add this to prevent crouch jumping
        {
			newScale.Y = 0.5f;

			if (player.IsOnFloor())
			{
				Vector3 slideDirection = velocity.Normalized();
				Vector3 slideVelocity = slideDirection * SPEED * DASH_MULTIPLIER;
				velocity += slideVelocity;
			}
        } else {
			newScale.Y = 1f;
		}

		// If the scale has changed, tween the scale to the new value
		if (newScale.Y != player.Hitbox.Scale.Y)
		{
			Tween tween = player.CreateTween();
			tween.SetTrans(Tween.TransitionType.Linear);
			tween.TweenProperty(player, "scale", newScale, 0.025f);
			tween.Play();
		}

		return velocity;
	}

    /// <summary>
    /// Returns a vector representing the desired movement direction based on the input event.
    /// All values are normalized to -1, 0, or 1.
    /// 
    /// For example, when trying to move forward the resulting vector will be (0, 0, -1) and when trying to jump the resulting vector will be (0, 1, 0).
    /// </summary>
    public static Vector3 GetDesiredMovementVector()
    {
        Vector3 movementVector = Vector3.Zero;

        if (Input.IsActionPressed("move_front"))
        {
            movementVector.Z -= 1;
        }

        if (Input.IsActionPressed("move_back"))
        {
            movementVector.Z += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            movementVector.X -= 1;
        }

        if (Input.IsActionPressed("move_right"))
        {
            movementVector.X += 1;
        }

        movementVector = movementVector.Normalized();

        return movementVector;
    }

}