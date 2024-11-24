namespace SneakGame;

using Godot;

public partial class PlayerMovementController : Node
{

	private Player _player;

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


	private bool SlideDurationExpired = true;


	public void Initialize(Player player)
	{
		_player = player;
	}

	public override void _Process(double delta)
	{
		if(_player == null) return;

		CameraController Camera = CameraController.Instance;
		
		Vector3 desiredMovement = GetDesiredMovementVector();
		if (Camera != null)
		{
			// TODO: Move to Player.cs
			if (Camera.FocusedEntity == null)
			{
				Camera.FocusedEntity = _player;
			}

			// Movement is based on the direction of the camera.
			// Eg - holding `move_left` will move towards the left of the camera and not the world origin
			desiredMovement = desiredMovement.Rotated(Vector3.Up, Camera.Rotation.Y);

			Vector3 newRotation = _player.Rotation;
			newRotation.Y = Camera.Rotation.Y + Mathf.Pi;
			_player.Rotation = newRotation;
		}
		
		ApplyPhysics(desiredMovement, delta);

		_player.MoveAndSlide();
	}

	private void ApplyPhysics(Vector3 desiredMovement, double delta)
	{
		Vector3 newVelocity = _player.Velocity;

		newVelocity = ApplyGravity(newVelocity, delta);
		newVelocity = ApplyMovement(desiredMovement, newVelocity, delta);

		_player.Velocity = newVelocity;
	}

	private Vector3 ApplyGravity(Vector3 velocity, double delta)
	{
		if (!AffectedByGravity) return velocity;

		if (_player.IsOnFloor())
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
		Vector3 movement = desiredMovement * SPEED;

		velocity.X = movement.X;
		velocity.Z = movement.Z;

		if (Input.IsActionPressed("move_jump") && _player.IsOnFloor())
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

		SetHitboxScaleY(newScale);
	}
	
	public void SetHitboxScaleY(float value)
	{
		if (_player.Hitbox == null) return;
		if (value == _player.Hitbox.Scale.Y) return;

		Vector3 newScale = _player.Hitbox.Scale;
		newScale.Y = value;

		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(_player.Hitbox, "scale", newScale, 0.025f);
		tween.Play();
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
		
		if (_player == null) return;
		if (value == false) return;

		GD.Print("Jumping");
		Vector3 velocity = _player.Velocity;
		velocity.Y += JUMP_VELOCITY;
		_player.Velocity = velocity;
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
