namespace SneakGame;

using Godot;

public partial class NonPlayerMovementController : Node
{

    [Export] public float Speed { get; set; } = 5f;
    [Export] public float Acceleration { get; set; } = 10f;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity { get; set; } = new(0, -9.8f, 0); // TODO: Should probably define this as a global constant (Godot might support this already). Having it separate might be good for variable gravity per location.
    [Export] public Vector3 TargetPosition {
        get => NavigationAgent.TargetPosition;
        set {
            NavigationAgent.TargetPosition = value;
        }
    }
    [Export] public NavigationAgent3D NavigationAgent { get; set; }


    private NonPlayer _nonPlayer;
    private MeshInstance3D _debugMesh;


    public void Initialize(NonPlayer nonPlayer)
    {
        _nonPlayer = nonPlayer;
        TargetPosition = _nonPlayer.GlobalPosition;
    }


    public override void _Process(double delta)
    {
        base._Process(delta);

        // Get the movement vectors
        Vector3 physicsVector = GetPhysicsVector();
        Vector3 desiredMovementVector = GetDesiredMovementVector();

        // Combine and apply all the vectors
        Vector3 desiredVelocity = MergeVectors([desiredMovementVector, physicsVector]);

        // Lerp the velocity
        _nonPlayer.Velocity = _nonPlayer.Velocity.Lerp(desiredVelocity, (float)(Acceleration * delta));

        // Actually move the character
        FaceTargetDirection();
        _nonPlayer.MoveAndSlide();

        DrawDebug();
    }

    /// <summary>
    /// Combine all the vectors in the queue and apply them to the player. We can then limit the player's speed by the MovementSpeed property.
    /// </summary>
    private static Vector3 MergeVectors(Vector3[] vectors)
    {
        if (vectors.Length == 0) return Vector3.Zero;

        Vector3 combinedVector = Vector3.Zero;

        foreach (Vector3 vector in vectors)
        {
            combinedVector += vector;
        }

        return combinedVector;
    }

    private Vector3 GetPhysicsVector()
    {
        Vector3 physicsVector = Vector3.Zero;

        if(AffectedByGravity && !_nonPlayer.IsOnFloor())
        {
            physicsVector.Y += Gravity.Y;
        }

        return physicsVector;
    }

    private Vector3 GetDesiredMovementVector()
    {
        if (!_nonPlayer.IsOnFloor()) return Vector3.Zero;

        Vector3 direction = _nonPlayer.GlobalPosition.DirectionTo(NavigationAgent.GetNextPathPosition());
        return direction * Speed;
    }

    private void FaceTargetDirection()
    {
        Vector3 direction = _nonPlayer.Velocity.Rotated(Vector3.Up, Mathf.DegToRad(180));

        direction.Y = 0; // Zero out the Y component so the character doesn't angle up or down

        if (!direction.IsZeroApprox())
        {
            direction = direction.Normalized(); // Normalize the direction vector
            _nonPlayer.LookAt(_nonPlayer.GlobalPosition + direction, Vector3.Up);
        }
    }

    // TODO: Investigate if I can replace these with methods from NavigationAgent3D
    public bool IsNearPosition(Vector3 position)
    {
        return IsNearPosition(position, 1f);
    }

    public bool IsNearPosition(Vector3 position, float distanceThreshold)
    {
        if (_nonPlayer == null) return false;

        float distanceToTarget = _nonPlayer.GlobalPosition.DistanceTo(position);

        return distanceToTarget <= distanceThreshold;
    }

    public bool HasReachedTarget()
    {
        return IsNearPosition(TargetPosition, 1f);
    }

    private void DrawDebug()
    {
        if (!Settings.GetInstance().DebugPathfinding) return; // Only show if Show Navigation debug is enabled

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
        shortTargetImmediateMesh.SurfaceAddVertex(_nonPlayer.GlobalPosition + meshOffset);
        shortTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset);
        shortTargetImmediateMesh.SurfaceEnd();

        // Draw a 1x1 square at TargetPosition
        ImmediateMesh internalTargetImmediateMesh = new();
        internalTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        internalTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(TargetPosition + meshOffset + new Vector3(-0.5f, 0, -0.5f));
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
