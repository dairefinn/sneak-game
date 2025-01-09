namespace SneakGame;

using Godot;

public partial class NonPlayerMovementController : Node
{

    [Export] public float MovementSpeed { get; set; } = 3;
	[Export] public int JumpVelocity = 5;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    [Export] public float StopThreshold { get; set; } = 2f;
    [Export] public Vector3 TargetPosition { get; set; }
    // [Export] public bool MoveToTarget { get; set; } = true; // If true, the NPC will move to the target position. If false, the NPC will stay in place.

    private NonPlayer _nonPlayer;
    private MeshInstance3D _debugMesh;
	private bool _crouching = false;
	private bool _jumping = false;
    private Vector3 internalTargetPosition; // When the target position is updated, we smoothly lerp it to the new one. This holds the actual current destination.

    private Vector3? previousPosition;
    private SceneTreeTimer hasMovedTimer;

    private RayCast3D collisionRayCast;


    public override void _Process(double delta)
    {
        base._Process(delta);

        ProcessHasMoved(previousPosition, _nonPlayer.GlobalTransform.Origin, TargetPosition);
        previousPosition = _nonPlayer.GlobalTransform.Origin;

        internalTargetPosition = GetTargetPositionLimitedBySpeed(delta);

        Vector3 desiredMovementDirection = GetDesiredMovementDirection();

        if (IsObstacleInPath(desiredMovementDirection))
        {
            GD.Print("Obstacle in path");
            desiredMovementDirection = FindNewPath(desiredMovementDirection, internalTargetPosition);
        }

        ApplyPhysics(desiredMovementDirection, delta);

        // Face the target position if not already facing itx
        Vector3 direction = _nonPlayer.GlobalPosition - internalTargetPosition;
        direction.Y = 0; // Zero out the Y component
        if (!direction.IsZeroApprox())
        {
            direction = direction.Normalized(); // Normalize the direction vector
            _nonPlayer.LookAt(_nonPlayer.GlobalPosition + direction, Vector3.Up);
        }

        _nonPlayer.MoveAndSlide();

        DrawDebug();
    }

    private bool IsObstacleInPath(Vector3 desiredMovementDirection)
    {
        if (collisionRayCast == null)
        {
            GD.Print("Creating new RayCast3D");
            // collisionRayCast = _nonPlayer.GetNode<RayCast3D>("RayCast3D");
            RayCast3D newRayCast = new();
            AddChild(newRayCast);

            collisionRayCast = newRayCast;
        }

        if (collisionRayCast == null) return false;

        collisionRayCast.TargetPosition = desiredMovementDirection * 2; // Cast the ray 2 units in the desired movement direction
        collisionRayCast.ForceRaycastUpdate(); // Force the raycast to update

        return collisionRayCast.IsColliding();
    }

    /// <summary>
    /// Find a new path around an obstacle. Reuses the IsObstacleInPath method to determine if the new path is clear. If a clear path is found after X amount of attempts, the original path is returned.
    /// </summary>
    /// <param name="desiredMovementDirection"></param>
    /// <param name="internalTargetPosition"></param>
    /// <returns></returns>
    private Vector3 FindNewPath(Vector3 desiredMovementDirection, Vector3 internalTargetPosition)
    {
        Vector3 newDirection = desiredMovementDirection;
        int attempts = 0;
        while (IsObstacleInPath(newDirection) && attempts < 10)
        {
            // Rotate the direction by 45 degrees
            newDirection = newDirection.Rotated(Vector3.Up, Mathf.DegToRad(45));
            attempts++;
        }

        if (attempts >= 10)
        {
            return desiredMovementDirection;
        }

        return newDirection;
    }

    private void ProcessHasMoved(Vector3? previousPosition, Vector3 currentPosition, Vector3 targetPosition)
    {
        if (previousPosition == null) return;

        float distanceThreshold = 1f; // Adjust this value as needed
        float distanceToTarget = currentPosition.DistanceTo(targetPosition);
        if (distanceToTarget <= distanceThreshold) return;

        bool hasMoved = previousPosition != currentPosition;
        if (hasMoved) {
            hasMovedTimer = null;
            return;
        }

        // At this point, the player hasn't moved. If the timer isn't already running, we need to start it.
        if (hasMovedTimer == null)
        {
            // GD.Print("Starting timer");
            hasMovedTimer = GetTree().CreateTimer(5);
            hasMovedTimer.Timeout += () => {
                // GD.Print("Hasn't moved in a while");
                TargetPosition = Vector3.Zero;
            };
        }
    }


    public Vector3 GetTargetPositionLimitedBySpeed(double delta)
    {
        Vector3 directionToTarget = _nonPlayer.GlobalTransform.Origin.DirectionTo(TargetPosition);
        float maximumDistance = Mathf.Min(MovementSpeed, _nonPlayer.GlobalTransform.Origin.DistanceTo(TargetPosition));
        Vector3 tempTargetPosition = _nonPlayer.GlobalTransform.Origin + (directionToTarget * maximumDistance);
        Vector3 newTarget = internalTargetPosition.MoveToward(tempTargetPosition, (float)delta * MovementSpeed * 2);
        return newTarget;
    }


    public void Initialize(NonPlayer nonPlayer)
    {
        _nonPlayer = nonPlayer;
        TargetPosition = _nonPlayer.GlobalTransform.Origin;
    }

	private void ApplyPhysics(Vector3 desiredMovement, double delta)
	{
		Vector3 newVelocity = _nonPlayer.Velocity;

		newVelocity = ApplyGravity(newVelocity, delta);
		newVelocity = ApplyMovement(desiredMovement, newVelocity, delta);
		newVelocity = ApplyJump(newVelocity, delta);

		_nonPlayer.Velocity = newVelocity;
	}

	private Vector3 ApplyGravity(Vector3 velocity, double delta)
	{
		if (!AffectedByGravity) return velocity;

		if (_nonPlayer.IsOnFloor())
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

    private Vector3 GetDesiredMovementDirection()
    {
        Vector3 desiredMovement = Vector3.Zero;

        Vector3 currentPosition = _nonPlayer.GlobalTransform.Origin;

        if (IsNearPosition(TargetPosition, StopThreshold)) return desiredMovement;

        // Get the normalized direction to the target position
        desiredMovement = (internalTargetPosition - currentPosition).Normalized();

        return desiredMovement;
    }

    public bool IsNearPosition(Vector3 position, float distanceThreshold = 5)
    {
        if (_nonPlayer == null) return false;
        return _nonPlayer.GlobalTransform.Origin.DistanceTo(position) <= distanceThreshold;
    }

    public void TargetNewRandomPositionOnMap()
    {
        // Get a random point between -50 and 50 on the x and z axis
        TargetPosition = new Vector3(
            GD.RandRange(-50, 50),
            0,
            GD.RandRange(-50, 50)
        );
    }

    public void ClearTargets()
    {
        TargetPosition = _nonPlayer.GlobalTransform.Origin;
    }

    public void DrawDebug()
    {
        if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        Vector3 meshOffset = new(0, 1f, 0); // Bumps the mesh up 1 unit so it's not in the ground. TODO: Might be a way to make it render through other objects instead.
        if (_debugMesh == null)
        {
            _debugMesh = new();
            GetTree().CurrentScene.AddChild(_debugMesh);
        }
        
        // Draw a line from the NPC to their current target
        ImmediateMesh shortTargetImmediateMesh = new();
        shortTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        shortTargetImmediateMesh.SurfaceAddVertex(_nonPlayer.GlobalTransform.Origin + meshOffset);
        shortTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset);
        shortTargetImmediateMesh.SurfaceEnd();

        // Draw a 1x1 square at internalTargetPosition
        ImmediateMesh internalTargetImmediateMesh = new();
        internalTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceEnd();

        // Draw a 1x1 square at TargetPosition
        ImmediateMesh targetPositionImmediateMesh = new();
        targetPositionImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        targetPositionImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        targetPositionImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f));
        targetPositionImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f));
        targetPositionImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        targetPositionImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        targetPositionImmediateMesh.SurfaceEnd();

        // Combine the meshes
        ArrayMesh combinedMesh = new();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, shortTargetImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Green });
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, internalTargetImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Red });
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, targetPositionImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Blue });
        
        _debugMesh.Mesh = combinedMesh;
    }

}
