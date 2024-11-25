namespace SneakGame;

using Godot;

public partial class NonPlayerMovementController : Node
{

    [Export] public float MovementSpeed { get; set; } = 5;
	[Export] public int JumpVelocity = 5;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    [Export] public float StopThreshold { get; set; } = 2f;
    [Export] public Vector3 TargetPosition { get; set; }

    private NonPlayer _nonPlayer;
    private MeshInstance3D _movementMesh;
	private bool _crouching = false;
	private bool _jumping = false;
    private Vector3 internalTargetPosition; // When the target position is updated, we smoothly lerp it to the new one. This holds the actual current destination.


    public override void _Process(double delta)
    {
        base._Process(delta);

        // Smoothly interpolate internalTargetPosition towards TargetPosition
        Vector3 directionToTarget = _nonPlayer.GlobalTransform.Origin.DirectionTo(TargetPosition);
        float maximumDistance = Mathf.Min(MovementSpeed, _nonPlayer.GlobalTransform.Origin.DistanceTo(TargetPosition));
        Vector3 tempTargetPosition = _nonPlayer.GlobalTransform.Origin + (directionToTarget * maximumDistance);
        internalTargetPosition = internalTargetPosition.MoveToward(tempTargetPosition, (float)delta * MovementSpeed * 2);

        Vector3 desiredMovementDirection = GetDesiredMovementDirection();

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
        Vector3 meshOffset = new(0, 1f, 0);
        if (_movementMesh == null)
        {
            _movementMesh = new();
            GetTree().CurrentScene.AddChild(_movementMesh);
        }
        
        // Draw a line from the NPC to their current target
        ImmediateMesh targetImmediateMesh = new();
        targetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        targetImmediateMesh.SurfaceAddVertex(_nonPlayer.GlobalTransform.Origin + meshOffset);
        targetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset);
        targetImmediateMesh.SurfaceEnd();

        // Draw a 1x1 square at internalTargetPosition
        ImmediateMesh internalTargetImmediateMesh = new();
        internalTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceEnd();

        // Combine the meshes
        ArrayMesh combinedMesh = new();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, targetImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Green });
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, internalTargetImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Red });
        
        _movementMesh.Mesh = combinedMesh;
    }

}
