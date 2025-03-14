namespace SneakGame;

using Godot;

public partial class PlayerMovementController : Node
{

	[ExportGroup("Abilities")]
	[Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public bool CanMove { get; set; } = true;
	[Export] public bool CanJump { get; set; } = true;
	[Export] public bool CanDoubleJump { get; set; } = true;
	[Export] public bool CanSprint { get; set; } = true;
	[Export] public bool CanCrouch { get; set; } = true;
	[Export] public bool CanDash { get; set; } = true;

	[ExportGroup("Physics")]
	[Export] public Vector3 Gravity = new(0, -25f, 0);
	[Export] public int MovementSpeed = 5;
	[Export] public int JumpVelocity = 10;
	[Export] public float JumpDuration = 0.5f;
	[Export] public float DoubleJumpThreshold = 1f;
	[Export] public float DashMultiplier = 5.0f;
	[Export] public float DashDuration = 0.5f;
	[Export] public float DashCooldown = 1.0f;
	[Export] public Vector3 CrouchModifier = new(1, 0.5f, 1);
	[Export] public float SprintSpeedMultiplier = 2.0f;

	[ExportGroup("References")]
	[Export] public CameraController CameraController { get; set; }
	[Export] public Player Player { get; set; }


	// Timers
	private SceneTreeTimer _jumpTimer;
	private SceneTreeTimer _doubleJumpTimer;
	private SceneTreeTimer _dashTimer;
	private SceneTreeTimer _dashCooldownTimer;

	// Ability states
	private bool _hasDoubleJumped = false;
	private bool _dashing = false;
	private bool _dashOnCooldown = false;
	private bool _sprinting = false;
	private bool _jumping = false;
	private bool _crouching = false;

	public Vector3 desiredMovement = Vector3.Zero;
	private Vector3 _targetPosition = Vector3.Zero;
	private MeshInstance3D _debugMesh;


	/// <summary>
	/// Called every physics frame. This is where all the movement logic is handled.
	/// </summary>
	/// <param name="delta">The time since the last frame.</param>
    public override void _Process(double delta)
	{
		if(Player == null) return; // Player must exist in order to move them
		
		_targetPosition = Player.GlobalTransform.Origin;
		bool isOnFloor = Player.IsOnFloor();

		_crouching = Input.IsActionPressed("move_crouch");
		desiredMovement = GetDesiredMovement(isOnFloor, _crouching);

		ApplyMovementVelocity(isOnFloor, desiredMovement, delta);
		ApplyAnimations(desiredMovement, isOnFloor, _crouching);
		DrawDebug();

		// Ensure the camera is updated every frame. This prevents jittery camera movement.
		CameraController?._Process(delta);
	}

	// This should apply:	
	// Aim walk backwards
	// Aim walk forwards
	// Aim walk left
	// Aim walk right
	// Crouch backwards
	// Crouch forwards
	// Crouch left
	// Crouch right
	// Crouch to stand
	// Dodge backwards
	// Dodge forwards
	// Dodge left
	// Dodge right
	// Fall
	// Idle
	// Jump running
	// Jump stationary
	// Kick
	// Land
	// Sprint backwards
	// Sprint end
	// Sprint forwards
	// Sprint forwards_001
	// Sprint left
	// Sprint right
	// Stand to crouch
	// Walk backwards
	// Walk forwards
	// Walk left
	// Walk right
	private void ApplyAnimations(Vector3 desiredMovement, bool isOnFloor, bool crouching)
	{
		if (Player.AnimationTree == null) return;

		Player.AnimationTree.Set("parameters/Crouch1/blend_amount", crouching ? 1 : 0);
		Player.AnimationTree.Set("parameters/Crouch2/blend_amount", crouching ? 1 : 0);
		Player.AnimationTree.Set("parameters/Strafe/blend_amount", desiredMovement.X != 0 ? 1 : 0);
		Player.AnimationTree.Set("parameters/Idle/blend_amount", desiredMovement == Vector3.Zero ? 1 : 0);
		
		// If moving backwards, reverse the "anim_walk_forwards" animation. This is not a blend node so we need to reverse the animation using the "Play mode" property.
		if (desiredMovement.Z > 0)
		{
			Player.AnimationTree.GetAnimation("Walk forwards").Set("speed_scale", -1);
			// GD.Print("Moving forwards: " + Player.AnimationTree.GetAnimation("Walk forwards").Get("speed_scale"));
		}
		else
		{
			Player.AnimationTree.GetAnimation("Walk forwards").Set("speed_scale", +1);
			// GD.Print("Moving backwards: " + Player.AnimationTree.GetAnimation("Walk forwards").Get("speed_scale"));
		}
	}

	/// <summary>
	/// Gets the desired movement direction as a 3D vector based on the inputs pressed.
	/// </summary>
	/// <param name="camera">The camera that the movement is based on. This is used to determine the direction of movement based on where the camera is facing.</param>
	/// <returns></returns>
	private Vector3 GetDesiredMovement(bool isOnFloor, bool crouching)
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
		
		// Normalize the movement vector to ensure consistent speed
		if (desiredMovement.Length() > 1)
		{
			desiredMovement = desiredMovement.Normalized();
		}

		if (!isOnFloor)
		{
			// velocity.Y += (float)(gravity.Y * delta);
			desiredMovement.Y = -1;
		}

		if (CanDoubleJump && Input.IsActionJustPressed("move_jump") && _jumping && !_hasDoubleJumped && _doubleJumpTimer != null && _doubleJumpTimer.TimeLeft >= 0)
		{
			GD.Print("Double jump!");
			_hasDoubleJumped = true;
			desiredMovement.Y = 1;
			StartJumpTimer();
		}

		if (CanJump && Input.IsActionJustPressed("move_jump") && isOnFloor)
		{
			GD.Print("Single jump!");
			_jumping = true;
			desiredMovement.Y = 1;
			StartJumpTimer();
		}
		
		if (CanDash && Input.IsActionJustPressed("move_dash") && !crouching && !_dashOnCooldown)
		{
			StartDashTimer();
		}
		
		if (CanSprint && Input.IsActionPressed("move_sprint") && (isOnFloor || _sprinting) && !crouching)
		{
			_sprinting = true;
			desiredMovement.X *= SprintSpeedMultiplier;
			desiredMovement.Z *= SprintSpeedMultiplier;
		}
		else
		{
			_sprinting = false;
		}

		return desiredMovement;
	}

	/// <summary>
	/// Applies all the different types of movements to the player.
	/// </summary>
	/// <param name="desiredMovement">Where the player wants to move based on the inputs pressed.</param>
	/// <param name="delta">The time since the last frame.</param>
	private void ApplyMovementVelocity(bool isOnFloor, Vector3 desiredMovement, double delta)
	{
		// Velocity
		Vector3 newVelocity = desiredMovement;
		newVelocity.X *= MovementSpeed;
		newVelocity.Z *= MovementSpeed;

		if (desiredMovement.Y > 0 || _jumping)
		{
			newVelocity.Y = JumpVelocity;
			float jumpProgress = (float)(_jumpTimer.TimeLeft / JumpDuration);
			newVelocity.Y *= jumpProgress;
		}
		if (desiredMovement.Y < 0 && !_jumping)
		{
			newVelocity.Y *= -Gravity.Y;
		}

		if (_dashing == true)
		{
			float dashTimerAdjusted = (float)(_dashTimer.TimeLeft / DashDuration) * DashMultiplier;
			newVelocity.X *= dashTimerAdjusted;
			newVelocity.Z *= dashTimerAdjusted;
		}

		// Movement is based on the direction of the camera.
		// Eg - holding `move_left` will move towards the left of the camera and not the world origin
		if (CameraController != null)
		{
			newVelocity = newVelocity.Rotated(Vector3.Up, CameraController.Rotation.Y);
		}

		Player.Velocity = newVelocity;

		_targetPosition = Player.GlobalTransform.Origin + (newVelocity * (float) delta * 10);

		// Transformations
		Vector3 hitboxScale = Vector3.One;
		if (CanCrouch)
		{
			hitboxScale = ApplyCrouchTransformation(hitboxScale, CrouchModifier);
		}
		SetHitboxScaleY(hitboxScale);

		Player.MoveAndSlide();
	}

	private void StartJumpTimer()
	{
		if (_jumpTimer != null && _hasDoubleJumped == true)
		{
			GD.Print("Jump timer already exists. Resetting.");
			_jumpTimer.Timeout -= OnJumpTimerTimeout;
			_jumpTimer.Dispose();
			_jumpTimer = null;
		}

		_jumpTimer = GetTree().CreateTimer(JumpDuration, true);
		_jumpTimer.Timeout += OnJumpTimerTimeout;

		_doubleJumpTimer = GetTree().CreateTimer(DoubleJumpThreshold, false);
		_doubleJumpTimer.Timeout += OnDoubleJumpTimerTimeout;
	}

	private void OnJumpTimerTimeout()
	{
		GD.Print("Jump timer ended.");
		_jumping = false;
	}

	private void OnDoubleJumpTimerTimeout()
	{
		GD.Print("Double jump timer ended.");
		_hasDoubleJumped = false;
	}

	private void StartDashTimer()
	{
		GD.Print("Starting dash");
		if (_dashTimer != null)
		{
			_dashTimer.Timeout -= OnDashTimerTimeout;
			_dashTimer.Dispose();
			_dashTimer = null;
			_dashCooldownTimer.Timeout -= OnDashCooldownTimerTimeout;
			_dashCooldownTimer.Dispose();
			_dashCooldownTimer = null;
		}

		_dashing = true;
		_dashOnCooldown = true;
		_dashTimer = GetTree().CreateTimer(DashDuration, false);
		_dashTimer.Timeout += OnDashTimerTimeout;
		_dashCooldownTimer = GetTree().CreateTimer(DashCooldown, false);
		_dashCooldownTimer.Timeout += OnDashCooldownTimerTimeout;
	}

	private void OnDashTimerTimeout()
	{
		GD.Print("Dash ended.");
		_dashing = false;
	}

	private void OnDashCooldownTimerTimeout()
	{
		GD.Print("Dash cooldown ended.");
		_dashOnCooldown = false;
	}

	/// <summary>
	/// Applies crouch transformation to the player if the crouch button is pressed.
	/// </summary>
	/// <param name="currentScale">The current scale of the player.</param>
	/// <param name="crouchModifier">The height to set the player to when crouching.</param>
	/// <returns></returns>
	private Vector3 ApplyCrouchTransformation(Vector3 currentScale, Vector3 crouchModifier)
	{
		if (_crouching)
		{
			return currentScale * crouchModifier;
		}

		return currentScale;
	}

	public void SetHitboxScaleY(Vector3 newScale)
	{
		if (Player.Hitbox == null) return;
		if (Player.Hitbox.Scale == newScale) return;

		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(Player.Hitbox, "scale", newScale, 0.025f);
		tween.Play();
	}


	private void DrawDebug()
	{
		if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        if (_debugMesh == null)
        {
            _debugMesh = new();
            GetTree().CurrentScene.AddChild(_debugMesh);
        }

		Vector3 playerHeight = new Vector3(0, 1.8f, 0);

        ArrayMesh combinedMesh = new();

		// Draw the projection of where we will move - can be used to visualize where we'll actually go without actually moving the character
        ImmediateMesh projectedMovementMeshBottom = new();
        projectedMovementMeshBottom.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        projectedMovementMeshBottom.SurfaceAddVertex(_targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshBottom.SurfaceAddVertex(_targetPosition + new Vector3(0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshBottom.SurfaceAddVertex(_targetPosition + new Vector3(0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshBottom.SurfaceAddVertex(_targetPosition + new Vector3(-0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshBottom.SurfaceAddVertex(_targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshBottom.SurfaceEnd();
		combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, projectedMovementMeshBottom.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Green });

        ImmediateMesh projectedMovementMeshTop = new();
        projectedMovementMeshTop.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        projectedMovementMeshTop.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshTop.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshTop.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshTop.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(-0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshTop.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
        projectedMovementMeshTop.SurfaceEnd();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, projectedMovementMeshTop.SurfaceGetArrays(0));

		ImmediateMesh cuboidEdge1 = new();
		cuboidEdge1.SurfaceBegin(Mesh.PrimitiveType.Lines);
		cuboidEdge1.SurfaceAddVertex(_targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge1.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge1.SurfaceEnd();
		combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, cuboidEdge1.SurfaceGetArrays(0));

		ImmediateMesh cuboidEdge2 = new();
		cuboidEdge2.SurfaceBegin(Mesh.PrimitiveType.Lines);
		cuboidEdge2.SurfaceAddVertex(_targetPosition + new Vector3(0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge2.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(0.5f, 0, -0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge2.SurfaceEnd();
		combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, cuboidEdge2.SurfaceGetArrays(0));

		ImmediateMesh cuboidEdge3 = new();
		cuboidEdge3.SurfaceBegin(Mesh.PrimitiveType.Lines);
		cuboidEdge3.SurfaceAddVertex(_targetPosition + new Vector3(-0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge3.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(-0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge3.SurfaceEnd();
		combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, cuboidEdge3.SurfaceGetArrays(0));

		ImmediateMesh cuboidEdge4 = new();
		cuboidEdge4.SurfaceBegin(Mesh.PrimitiveType.Lines);
		cuboidEdge4.SurfaceAddVertex(_targetPosition + new Vector3(0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge4.SurfaceAddVertex(playerHeight + _targetPosition + new Vector3(0.5f, 0, 0.5f).Rotated(Vector3.Up, Player.Rotation.Y));
		cuboidEdge4.SurfaceEnd();
		combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, cuboidEdge4.SurfaceGetArrays(0));
        
        _debugMesh.Mesh = combinedMesh;
	}

}
