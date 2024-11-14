namespace SneakGame;

using Godot;

public partial class Player : CharacterBody3D
{

    public static readonly int SPEED_CONSTANT = 10;
    public static readonly int JUMP_HEIGHT = 5;
    public static readonly int CROUCH_HEIGHT = 2;

	// TODO: Replace with actual gravity value when I figure out where to put it
	public static readonly Vector3 GRAVITY_VECTOR = new(0, -9.8f, 0);

	[Export] public CharacterStats Stats { get; set; }

	[Export] public bool Gravity { get; set; }


	private CameraController Camera { get; set; }
	public CollisionShape3D Hitbox { get; set; }


	public override void _Ready()
	{
		base._Ready();

		Hitbox = GetNode<CollisionShape3D>("Hitbox");
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

		if (Camera == null)
		{
			Camera = CameraController.Instance;
		}
		
		Vector3 desiredMovement = GetDesiredMovementVector();
		if (Camera != null)
		{
			// Movement is based on the direction of the camera.
			// Eg - holding `move_left` will move towards the left of the camera and not the world origin
			desiredMovement = desiredMovement.Rotated(Vector3.Up, Camera.Rotation.Y);

			// TODO: Placeholder for now
			Vector3 newRotation = Rotation;
			newRotation.Y = Camera.Rotation.Y + Mathf.Pi;
			Rotation = newRotation;
		}
		
		ApplyPhysics(desiredMovement, delta);

		MoveAndSlide();
    }

	private void ApplyPhysics(Vector3 desiredMovement, double delta)
	{
		Vector3 newVelocity = Velocity;

		newVelocity = ApplyGravity(newVelocity, delta);
		newVelocity = ApplyMovement(desiredMovement, newVelocity, delta);

		Velocity = newVelocity;
	}

	private Vector3 ApplyGravity(Vector3 velocity, double delta)
	{
		if (!Gravity) return velocity;

		if (IsOnFloor())
		{
			velocity.Y = 0;
		}
		else
		{
			velocity.Y += (float)(GRAVITY_VECTOR.Y * delta);
		}

		return velocity;
	}

	public Vector3 ApplyMovement(Vector3 desiredMovement, Vector3 velocity, double delta)
	{
		Vector3 movement = desiredMovement * SPEED_CONSTANT;

		velocity.X = movement.X;
		velocity.Z = movement.Z;

        if (Input.IsActionPressed("move_jump") && IsOnFloor())
        {
            velocity = Jump(velocity, delta);
        }

		velocity = CrouchOrSlide(velocity, delta);

		return velocity;
	}

	public Vector3 Jump(Vector3 velocity, double delta)
	{
		if (IsOnFloor())
		{
			velocity.Y = JUMP_HEIGHT;
		}

		return velocity;
	}

	public Vector3 CrouchOrSlide(Vector3 velocity, double delta)
	{
		Vector3 newScale = Hitbox.Scale;

        if (Input.IsActionPressed("move_crouch")) // && IsOnFloor() <-- Add this to prevent crouch jumping
        {
			newScale.Y = 0.5f;
        } else {
			newScale.Y = 1f;
		}

		// If the scale has changed, tween the scale to the new value
		if (newScale.Y != Hitbox.Scale.Y)
		{
			Tween tween = CreateTween();
			tween.SetTrans(Tween.TransitionType.Linear);
			tween.TweenProperty(this, "scale", newScale, 0.025f);
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
