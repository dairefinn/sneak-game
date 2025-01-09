namespace SneakGame;

using Godot;

public partial class PlayerMovementController : Node
{

	private CameraController _camera { get; set; }
	private Player _player { get; set; }

	
	[ExportGroup("Abilities")]
	[Export] public bool CanDoubleJump { get; set; } = true;
	[Export] public bool CanSprint { get; set; } = true;
	[Export] public bool CanSlide { get; set; } = true;
	[Export] public bool CanCrouch { get; set; } = true;
	[Export] public bool CanDash { get; set; } = true;
	// TODO: Implement these
	[Export] public bool CanWallClimb { get; set; } = true;
	[Export] public bool CanWallRun { get; set; } = true;


	[ExportGroup("Physics")]
	[Export] public int MovementSpeed = 10;
	[Export] public float SlideMultiplier = 2.0f;
	[Export] public float SlideDuration = 1.0f;
	[Export] public int JumpVelocity = 5;
	[Export] public float CrouchHeight = 0.5f;
	[Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new Vector3(0, -9.8f, 0);
	[Export] public float SprintSpeedMultiplier = 2.0f;

	private bool _crouching = false;
	private bool _sprinting = false;
	private bool _jumping = false;
	private bool _hasDoubleJumped = false;
	private bool _dashing = false;


	private Vector3 _targetPosition;
	private SceneTreeTimer _slideTimer;
	private MeshInstance3D _debugMesh;


	public void Initialize(Player player, CameraController camera)
	{
		_player = player;
		_camera = camera;
	}

    public override void _Process(double delta)
	{
		if(_player == null) return;

		if (_player.IsOnFloor())
		{
			_hasDoubleJumped = false;
		}

		// Stick the camera to the player if it's not focused on anything. Might make more sense to have this elsewhere.
		if (_camera != null)
		{
			if (_camera.FocusedEntity == null)
			{
				_camera.FocusedEntity = _player;
			}
		}

		Vector3 desiredMovement = GetDesiredMovement(_camera);

		_jumping = Input.IsActionJustPressed("move_jump");
		_crouching = Input.IsActionPressed("move_crouch");
		_sprinting = Input.IsActionPressed("move_sprint");
		_dashing = Input.IsActionJustPressed("move_dash");

		ApplyPhysics(desiredMovement, delta);
		ApplyTransformations(delta);

		_player.MoveAndSlide();

		DrawDebug();
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
		newVelocity = ApplySprint(newVelocity, delta);
		newVelocity = ApplyDash(newVelocity, delta);

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

		if (_player.IsOnFloor())
		{
			velocity.Y += JumpVelocity;
			return velocity;
		}

		if (!_hasDoubleJumped)
		{
			_hasDoubleJumped = true;
			velocity.Y += JumpVelocity;
			return velocity;
		}


		return velocity;
	}

	private Vector3 ApplySprint(Vector3 velocity, double delta)
	{
		if (!_sprinting) return velocity;
		if (!_player.IsOnFloor()) return velocity;

		velocity.X *= SprintSpeedMultiplier;
		velocity.Z *= SprintSpeedMultiplier;

		return velocity;
	}

	// TODO: The slide speed should taper off over time
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

	// TODO: This is jumpy. Maybe find movement direction and apply a force in that direction instead
	private Vector3 ApplyDash(Vector3 velocity, double delta)
	{
		if (!CanDash) return velocity;
		if (!_dashing) return velocity;

		Vector3 movementDirection = GetDesiredMovement(_camera);
		movementDirection.Y = 0;

		velocity = movementDirection * MovementSpeed * 10;

		return velocity;
	}

	private bool CheckSlideConditions(Vector3 velocity)
	{
		if (!CanSlide) return false; // Must be allowed to slide
		if (!_crouching) return false; // Must be crouching
		if (!_player.IsOnFloor()) return false; // Must be on the floor
		if (velocity.Length() == 0) return false; // Must be moving
		if (velocity.Length() < MovementSpeed) return false; // Must be moving faster than the base speed

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
		float newScaleY = 1.0f;

		if (_crouching && CanCrouch)
		{
			newScaleY = CrouchHeight;
		}

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

	private void DrawDebug()
	{
		if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        Vector3 meshOffset = new(0, 1f, 0); // Bumps the mesh up 1 unit so it's not in the ground. TODO: Might be a way to make it render through other objects instead.
        if (_debugMesh == null)
        {
            _debugMesh = new();
            GetTree().CurrentScene.AddChild(_debugMesh);
        }

        ArrayMesh combinedMesh = new();

		// Draw the projection of where we will move - can be used to visualize where we'll actually go without actually moving the character
        ImmediateMesh projectedMovementMesh = new();
        projectedMovementMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        projectedMovementMesh.SurfaceEnd();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, projectedMovementMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Green });
        
        _debugMesh.Mesh = combinedMesh;
	}

}
