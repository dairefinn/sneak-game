namespace SneakGame;

using System.Collections.Generic;
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
	// TODO: Implement these
	[Export] public bool CanWallClimb { get; set; } = true;
	[Export] public bool CanWallRun { get; set; } = true;

	[ExportGroup("Physics")]
	[Export] public Vector3 Gravity = new(0, -25f, 0);
	[Export] public int MovementSpeed = 10;
	[Export] public int JumpVelocity = 10;
	[Export] public float JumpDuration = 0.5f;
	[Export] public float DoubleJumpThreshold = 1f;
	[Export] public float DashMultiplier = 5.0f;
	[Export] public float DashDuration = 0.5f;
	[Export] public Vector3 CrouchModifier = new(1, 0.5f, 1);
	[Export] public float SprintSpeedMultiplier = 2.0f;

	// References
	private CameraController _camera { get; set; }
	private CharacterBody3D _player { get; set; }
	private CollisionShape3D _hitbox { get; set; }

	// Timers
	private SceneTreeTimer _jumpTimer;
	private SceneTreeTimer _doubleJumpTimer;
	private SceneTreeTimer _dashTimer;
	
	// Ability states
	private bool _hasDoubleJumped = false;
	private bool _dashing = false;
	private bool _sprinting = false;
	private bool _jumping = false;
	private bool _crouching = false;

	private Vector3 _targetPosition = Vector3.Zero;
	private MeshInstance3D _debugMesh;


	/// <summary>
	/// Instantiates any references that are needed for the controller to function.
	/// </summary>
	/// <param name="player">The player that this movement controller is moving.</param>
	/// <param name="camera">The camera that is attached to this movement controller.</param>
	public void Initialize(CharacterBody3D player, CollisionShape3D hitbox, CameraController camera)
	{
		_player = player;
		_hitbox = hitbox;
		_camera = camera;

		// Stick the camera to the player if it's not focused on anything. Might make more sense to have this elsewhere.
		if (_camera != null)
		{
			if (_camera.FocusedEntity == null)
			{
				_camera.FocusedEntity = _player;
			}
		}
	}

	/// <summary>
	/// Called every physics frame. This is where all the movement logic is handled.
	/// </summary>
	/// <param name="delta">The time since the last frame.</param>
    public override void _Process(double delta)
	{
		if(_player == null) return; // Player must exist in order to move them
		
		_targetPosition = _player.GlobalTransform.Origin;
		bool isOnFloor = _player.IsOnFloor();

		// if (isOnFloor)
		// {
		// 	_hasDoubleJumped = false;
		// 	_jumping = false;
		// }

		_crouching = Input.IsActionPressed("move_crouch");
		Vector3 desiredMovement = GetDesiredMovement(isOnFloor, _crouching);

		ApplyMovementVelocity(_player, isOnFloor, desiredMovement, _camera, delta);

		DrawDebug();

		// Ensure the camera is updated every frame. This prevents jittery camera movement.
		_camera._Process(delta);
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
		
		if (CanDash && Input.IsActionJustPressed("move_dash") && !crouching)
		{
			StartDashTimer();
		}
		
		if (CanSprint && Input.IsActionPressed("move_sprint") && isOnFloor && !crouching)
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
	private void ApplyMovementVelocity(CharacterBody3D player, bool isOnFloor, Vector3 desiredMovement, CameraController camera, double delta)
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
		if (camera != null)
		{
			newVelocity = newVelocity.Rotated(Vector3.Up, camera.Rotation.Y);
		}

		player.Velocity = newVelocity;

		_targetPosition = _player.GlobalTransform.Origin + (desiredMovement.Rotated(Vector3.Up, camera.Rotation.Y));
		// _targetPosition = _player.GlobalTransform.Origin + newVelocity;

		// Transformations
		Vector3 hitboxScale = Vector3.One;
		if (CanCrouch)
		{
			hitboxScale = ApplyCrouchTransformation(hitboxScale, CrouchModifier);
		}
		SetHitboxScaleY(hitboxScale);

		player.MoveAndSlide();
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
		}

		_dashing = true;
		_dashTimer = GetTree().CreateTimer(DashDuration, false);
		_dashTimer.Timeout += OnDashTimerTimeout;
	}

	private void OnDashTimerTimeout()
	{
		GD.Print("Dash ended.");
		_dashing = false;
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
		if (_hitbox == null) return;
		if (_hitbox.Scale == newScale) return;

		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(_hitbox, "scale", newScale, 0.025f);
		tween.Play();
	}


	private void DrawDebug()
	{
		if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        Vector3 meshOffset = new(0, 2, 0); // Bumps the mesh up 1 unit so it's not in the ground. TODO: Might be a way to make it render through other objects instead.
        if (_debugMesh == null)
        {
            _debugMesh = new();
            GetTree().CurrentScene.AddChild(_debugMesh);
        }

        ArrayMesh combinedMesh = new();

		List<Vector3> square = new()
		{
			new Vector3(-0.5f, 0, -0.5f),
			new Vector3(0.5f, 0, -0.5f),
			new Vector3(0.5f, 0, 0.5f),
			new Vector3(-0.5f, 0, 0.5f),
			new Vector3(-0.5f, 0, -0.5f)
		};

		// Draw the projection of where we will move - can be used to visualize where we'll actually go without actually moving the character
        ImmediateMesh projectedMovementMesh = new();
        projectedMovementMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, _player.Rotation.Y));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f).Rotated(Vector3.Up, _player.Rotation.Y));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f).Rotated(Vector3.Up, _player.Rotation.Y));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f).Rotated(Vector3.Up, _player.Rotation.Y));
        projectedMovementMesh.SurfaceAddVertex(_targetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f).Rotated(Vector3.Up, _player.Rotation.Y));
        projectedMovementMesh.SurfaceEnd();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, projectedMovementMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Green });
        
        _debugMesh.Mesh = combinedMesh;
	}

}
