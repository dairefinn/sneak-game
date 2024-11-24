namespace SneakGame;

using Godot;

public partial class PlayerMovementController : Node
{

	private CameraController _camera { get; set; }
	private Player _player { get; set; }


	[ExportGroup("Physics")]
	[Export] public int MovementSpeed = 10;
	[Export] public float SlideMultiplier = 2.0f;
	[Export] public float SlideDuration = 1.0f;
	[Export] public int JumpVelocity = 5;
	[Export] public float CrouchHeight = 0.5f;
	[Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new Vector3(0, -9.8f, 0);

	private bool _crouching = false;
	private bool _sprinting = false;
	private bool _jumping = false;

	private SceneTreeTimer _slideTimer;


	public void Initialize(Player player, CameraController camera)
	{
		_player = player;
		_camera = camera;
	}

    public override void _Process(double delta)
	{
		if(_player == null) return;

		// Stick the camera to the player if it's not focused on anything. Might make more sense to have this elsewhere.
		if (_camera != null)
		{
			if (_camera.FocusedEntity == null)
			{
				_camera.FocusedEntity = _player;
			}

			Vector3 newRotation = _player.Rotation;
			newRotation.Y = _camera.Rotation.Y + Mathf.Pi;
			_player.Rotation = newRotation;
		}

		Vector3 desiredMovement = GetDesiredMovement(_camera);

		_jumping = Input.IsActionPressed("move_jump") && _player.IsOnFloor();
		_crouching = Input.IsActionPressed("move_crouch");

		ApplyPhysics(desiredMovement, delta);
		ApplyTransformations(delta);

		_player.MoveAndSlide();
	}

	private Vector3 GetDesiredMovement(CameraController camera)
	{
		Vector3 desiredMovement = Vector3.Zero;

		if (Input.IsActionPressed("move_front"))
		{
			desiredMovement.Z -= 1;
		}

		if (Input.IsActionPressed("move_back"))
		{
			desiredMovement.Z += 1;
		}

		if (Input.IsActionPressed("move_left"))
		{
			desiredMovement.X -= 1;
		}

		if (Input.IsActionPressed("move_right"))
		{
			desiredMovement.X += 1;
		}

		desiredMovement = desiredMovement.Normalized();
		
		// Movement is based on the direction of the camera.
		// Eg - holding `move_left` will move towards the left of the camera and not the world origin
		if (camera != null)
		{
			desiredMovement = desiredMovement.Rotated(Vector3.Up, camera.Rotation.Y);
		}

		return desiredMovement;
	}


	private void ApplyPhysics(Vector3 desiredMovement, double delta)
	{
		Vector3 newVelocity = _player.Velocity;

		newVelocity = ApplyGravity(newVelocity, delta);
		newVelocity = ApplyMovement(desiredMovement, newVelocity, delta);
		newVelocity = ApplyJump(newVelocity, delta);
		newVelocity = ApplySlide(newVelocity, delta);

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
			velocity.Y += (float)(Gravity.Y * delta);
		}

		return velocity;
	}

	public Vector3 ApplyMovement(Vector3 desiredMovement, Vector3 velocity, double delta)
	{
		Vector3 movement = desiredMovement * MovementSpeed;

		velocity.X = movement.X;
		velocity.Z = movement.Z;

		return velocity;
	}

	private Vector3 ApplyJump(Vector3 velocity, double delta)
	{
		if (!_jumping) return velocity;

		velocity.Y += JumpVelocity;

		return velocity;
	}

	private Vector3 ApplySlide(Vector3 velocity, double delta)
	{
		if (!CheckSlideConditions(velocity))
		{
			_slideTimer = null;
			return velocity;
		}

		if (_slideTimer == null)
		{
            _slideTimer = GetTree().CreateTimer(SlideDuration, false);
		}
		else if (_slideTimer.TimeLeft <= 0)
		{
			return velocity;
		}

		velocity.X *= SlideMultiplier;
		velocity.Z *= SlideMultiplier;

		return velocity;
	}

	private bool CheckSlideConditions(Vector3 velocity)
	{
		if (!_crouching) return false; // Must be crouching
		if (!_player.IsOnFloor()) return false; // Must be on the floor

		// Must be moving in a direction
		Vector3 velocityHorizontalOnly = velocity;
		velocityHorizontalOnly.Y = 0;
		if (velocityHorizontalOnly.Length() == 0) return false;

		return true;
	}


	private void ApplyTransformations(double delta)
	{
		ApplyCrouch();
	}

	private void ApplyCrouch()
	{
		float newScaleY = _crouching ? CrouchHeight : 1.0f;
		SetHitboxScaleY(newScaleY);
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

}
