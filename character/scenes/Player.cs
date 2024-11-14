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

		if (desiredMovement.Y > 0 && IsOnFloor())
		{
			velocity = Jump(velocity, delta);
		}
		
		if (desiredMovement.Y < 0)
		{
			velocity = Crouch(velocity, delta);
		} else {
			Hitbox.Scale = Vector3.One;
		}

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

	public Vector3 Crouch(Vector3 velocity, double delta)
	{
		if (IsOnFloor())
		{
			velocity.Y = -CROUCH_HEIGHT;
			Hitbox.Scale = new Vector3(1, 0.5f, 1);
		}

		return velocity;
	}

    /// <summary>
    /// Returns a vector representing the desired movement direction based on the input event.
    /// All values are normalized to -1, 0, or 1.
    /// 
    /// For example, when trying to move forward the resulting vector will be (0, 0, -1) and when trying to jump the resulting vector will be (0, 1, 0).
    /// </summary>
	// TODO: This works fine but might need to be re-thought if we want crouch jumping or other more complex movement mechanics
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

        if (Input.IsActionPressed("move_jump"))
        {
            movementVector.Y += 1;
        }

        if (Input.IsActionPressed("move_crouch"))
        {
            movementVector.Y -= 1;
        }

        movementVector = movementVector.Normalized();

        return movementVector;
    }
}
