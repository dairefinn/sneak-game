#nullable enable

namespace SneakGame;

using Godot;

[GlobalClass]
public partial class PlayerMovementController : Resource
{

	[ExportGroup("Physics")]
	[Export] public int SPEED = 10;
	[Export] public int SLIDE_MULTIPLIER = 2;
	[Export] public float SLIDE_DURATION = 5.0f;
	[Export] public int JUMP_VELOCITY = 5;
	[Export] public int CROUCH_HEIGHT = 2;
	[Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 GRAVITY_VECTOR = new(0, -9.8f, 0); // TODO: Replace with actual gravity value when I figure out where to put it
	
	[Export] public bool Crouching {
		get => _crouching;
		set => SetCrouching(value);
	}
	private bool _crouching = false;

	[Export] public bool Sliding {
		get => _sliding;
		set => SetSliding(value);
	}
	private bool _sliding = false;

	[Export] public bool Sprinting {
		get => _sprinting;
		set => SetSprinting(value);
	}
	private bool _sprinting = false;

	[Export] public bool Jumping {
		get => _jumping;
		set => SetJumping(value);
	}
	private bool _jumping = false;



	// TODO: Make this a Node3D export variable instead
	private Player? Player;
	
	// TODO: Implement this
	// private Node3D Hitbox;

	private bool SlideDurationExpired = true;


	public void Initialize(Player player)
	{
		Player = player;
	}

	public void OnProcess(double delta)
	{
		if(Player == null) return;

		CameraController Camera = CameraController.Instance;
		
		Vector3 desiredMovement = GetDesiredMovementVector();
		if (Camera != null)
		{
			// TODO: Move to Player.cs
			if (Camera.FocusedEntity == null)
			{
				Camera.FocusedEntity = Player;
			}

			// Movement is based on the direction of the camera.
			// Eg - holding `move_left` will move towards the left of the camera and not the world origin
			desiredMovement = desiredMovement.Rotated(Vector3.Up, Camera.Rotation.Y);

			Vector3 newRotation = Player.Rotation;
			newRotation.Y = Camera.Rotation.Y + Mathf.Pi;
			Player.Rotation = newRotation;
		}
		
		ApplyPhysics(Player, desiredMovement, delta);

		Player.MoveAndSlide();
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

		if (Input.IsActionPressed("move_jump") && player.IsOnFloor())
		{
			Jumping = true;
		}

		if (Input.IsActionPressed("move_crouch"))
		{
			Crouching = true;
			Sliding = true;
		}
		else
		{
			Crouching = false;
			Sliding = false;
		}

		return velocity;
	}

	public void SetCrouching(bool value)
	{
		_crouching = value;

		float newScale = 1.0f;

		if (value == true)
		{
			newScale = 0.5f;
		}

		Player?.SetHitboxScaleY(newScale);
	}

	private void SetSliding(bool value)
	{
		_sliding = value;

		// Vector3 slideDirection = Player.Velocity.Normalized();
		// Vector3 slideVelocity = slideDirection * SPEED * SLIDE_MULTIPLIER;
		// Player.Velocity += slideVelocity;

		// if (value == true)
		// {
		// 	Player.GetTree().CreateTimer(SLIDE_DURATION, false).Timeout += () => {
		// 		SlideDurationExpired = true;
		// 	};
		// }
		// else
		// {
		// 	SlideDurationExpired = true;
		// }
	}

	private void SetSprinting(bool value)
	{

	}

	private void SetJumping(bool value)
	{
		_jumping = value;
		
		if (Player == null) return;
		if (value == false) return;

		GD.Print("Jumping");
		Vector3 velocity = Player.Velocity;
		velocity.Y += JUMP_VELOCITY;
		Player.Velocity = velocity;
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
