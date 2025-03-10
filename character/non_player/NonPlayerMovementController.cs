namespace SneakGame;

using Godot;

public partial class NonPlayerMovementController : Node
{

    [Export] public float MovementSpeed { get; set; } = 3;
	[Export] public int JumpVelocity = 5;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new(0, -9.8f, 0);
    
    [Export] public float StopThreshold { get; set; } = 1f; // The distance from the target position at which the player will consider themselves to have reached it
    [Export] public Vector3 TargetPosition {
        get => _targetPosition;
        set {
            if (_targetPosition == value) return;
            _targetPosition = value;
            internalTarget = value;
        }
    }
    private Vector3 _targetPosition;
    [Export ]public RayCast3D collisionRayCast;


    private NonPlayer _nonPlayer;
    private MeshInstance3D _debugMesh;
    private Vector3? positionStartOfFrame = null;
    private Vector3? internalTarget = Vector3.Zero;


    public void Initialize(NonPlayer nonPlayer)
    {
        _nonPlayer = nonPlayer;
        TargetPosition = _nonPlayer.GlobalPosition;
    }


    public override void _Process(double delta)
    {
        base._Process(delta);

        if (internalTarget == null)
        {
            internalTarget = TargetPosition;
        }

        positionStartOfFrame = _nonPlayer.GlobalPosition;

        // Get the movement vectors
        Vector3 physicsVector = GetPhysicsVector();
        Vector3 desiredMovementVector = GetDesiredMovementVector();

        // Combine and apply all the vectors
        Vector3 desiredVelocity = MergeVectors([desiredMovementVector, physicsVector]);
        _nonPlayer.Velocity = desiredVelocity;

        // Lerp the velocity
        // _nonPlayer.Velocity = _nonPlayer.Velocity.LinearInterpolate(desiredVelocity, 0.1f);

        // Collison handling
        if (positionStartOfFrame != null && collisionRayCast.IsColliding())
        {
            float distanceToTargetPrev = positionStartOfFrame.Value.DistanceTo(internalTarget.Value);
            float distanceToTargetCurr = _nonPlayer.GlobalPosition.DistanceTo(internalTarget.Value);

            float differenceInDistances = distanceToTargetPrev - distanceToTargetCurr;

            if (differenceInDistances < 0.1f)
            {
                internalTarget = FindNewPath(internalTarget.Value);
            }
        } else if (IsNearPosition(internalTarget.Value)) {
            internalTarget = TargetPosition;
        }

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

        // Limit the speed of the player
        // FIXME: This makes the player fall at the movement speed
        // combinedVector = combinedVector.Normalized() * MovementSpeed;

        return combinedVector;
    }

    private Vector3 GetPhysicsVector()
    {
        Vector3 physicsVector = Vector3.Zero;

        if (_nonPlayer.IsOnFloor())
        {
            physicsVector.Y = 0;
        }
        else
        {
            physicsVector.Y += Gravity.Y;
        }

        return physicsVector;
    }

    private Vector3 GetDesiredMovementVector()
    {
        Vector3 desiredMovement = Vector3.Zero;
        Vector3 currentPosition = _nonPlayer.GlobalPosition;
        Vector3 targetHorizontal = new Vector3(internalTarget.Value.X, 0, internalTarget.Value.Z);

        if (IsNearPosition(targetHorizontal, StopThreshold)) return desiredMovement;

        Vector3 directionToTarget = (targetHorizontal - currentPosition).Normalized();
        
        desiredMovement = directionToTarget * MovementSpeed;

        return desiredMovement;
    }

    private void FaceTargetDirection()
    {
        Vector3 direction = _nonPlayer.GlobalPosition - internalTarget.Value;

        direction.Y = 0; // Zero out the Y component so the character doesn't angle up or down

        if (!direction.IsZeroApprox())
        {
            direction = direction.Normalized(); // Normalize the direction vector
            _nonPlayer.LookAt(_nonPlayer.GlobalPosition + direction, Vector3.Up);
        }
    }

    public bool IsNearPosition(Vector3 position)
    {
        return IsNearPosition(position, StopThreshold);
    }

    public bool IsNearPosition(Vector3 position, float distanceThreshold)
    {
        if (_nonPlayer == null) return false;

        float distanceToTarget = _nonPlayer.GlobalPosition.DistanceTo(position);

        return distanceToTarget <= distanceThreshold;
    }

    public bool HasReachedTarget()
    {
        return IsNearPosition(TargetPosition, StopThreshold);
    }

    public void ClearTargets()
    {
        TargetPosition = _nonPlayer.GlobalPosition;
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
        
        // Draw a line from the NPC to their current target
        ImmediateMesh shortTargetImmediateMesh = new();
        shortTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        shortTargetImmediateMesh.SurfaceAddVertex(_nonPlayer.GlobalPosition + meshOffset);
        shortTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset);
        shortTargetImmediateMesh.SurfaceEnd();

        // Draw a 1x1 square at TargetPosition
        ImmediateMesh internalTargetImmediateMesh = new();
        internalTargetImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        internalTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset + new Vector3(-0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset + new Vector3(0.5f, 0, -0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset + new Vector3(0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset + new Vector3(-0.5f, 0, 0.5f));
        internalTargetImmediateMesh.SurfaceAddVertex(internalTarget.Value + meshOffset + new Vector3(-0.5f, 0, -0.5f));
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

    /// <summary>
    /// Find a new path around an obstacle. Reuses the IsObstacleInPath method to determine if the new path is clear. If a clear path is found after X amount of attempts, the original path is returned.
    /// </summary>
    /// <param name="desiredMovementDirection"></param>
    /// <param name="internalTargetPosition"></param>
    /// <returns></returns>
    private Vector3 FindNewPath(Vector3 internalTarget)
    {
        int INCREMENT = 45;
        int degreesChecked = 0;

        while (collisionRayCast.IsColliding() && degreesChecked < 360)
        {
            // Target to the right of the player and keep rotating 45 degrees until a clear path is found
            collisionRayCast.RotateY(INCREMENT);
            collisionRayCast.ForceRaycastUpdate();
            degreesChecked += INCREMENT;
        }

        collisionRayCast.Rotation = Vector3.Zero; // Reset the rotation

        if (degreesChecked >= 360)
        {
            return internalTarget;
        }
        else
        {
            GD.Print("Found new path");
            Vector3 newTarget = _nonPlayer.GlobalPosition + collisionRayCast.TargetPosition.Rotated(Vector3.Up, Mathf.DegToRad(degreesChecked));
            return newTarget;
        }
    }

}
