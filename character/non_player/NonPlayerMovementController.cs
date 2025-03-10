namespace SneakGame;

using System;
using Godot;
using Godot.Collections;

public partial class NonPlayerMovementController : Node
{

    [Export] public float MovementSpeed { get; set; } = 3;
	[Export] public int JumpVelocity { get; set; } = 5;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity { get; set; } = new(0, -9.8f, 0);
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

        internalTarget = HandleCollisions(internalTarget.Value);

        // Get the movement vectors
        Vector3 physicsVector = GetPhysicsVector();
        Vector3 desiredMovementVector = GetDesiredMovementVector();

        // Combine and apply all the vectors
        Vector3 desiredVelocity = MergeVectors([desiredMovementVector, physicsVector]);

        // Lerp the velocity
        _nonPlayer.Velocity = _nonPlayer.Velocity.Lerp(desiredVelocity, 0.1f);

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
        // Vector3 direction = _nonPlayer.GlobalPosition - internalTarget.Value;
        Vector3 direction = _nonPlayer.Velocity.Rotated(Vector3.Up, Mathf.DegToRad(180));

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


    // TODO: Instead of rotating the character, draw a path between the target and the character to find a way to reach it.
    // Will probably need to replace the TargetPosition with an actual path to follow.
    // This works for now but if the NPC enters a corner they'll just follow the wall to get out and it looks bad.
    private RayCast3D collisionDetectionRay;
    private Vector3 HandleCollisions(Vector3 targetPosition)
    {
        // This will reset the NPC back to their original target if they reach the point they were given to avoid the obstacle
        if (IsNearPosition(internalTarget.Value))
        {
            return TargetPosition;
        }

        float DEGREE_STEP = 10f;
        Vector3 currentPosition = _nonPlayer.GlobalPosition;

        // Check if there's an obstacle in the way
        if (IsObstacleInWay())
        {
            return currentPosition + new Vector3(0, 0, 2).Rotated(Vector3.Up, _nonPlayer.Rotation.Y + Mathf.DegToRad(DEGREE_STEP));
        }

        return targetPosition;
    }

    private bool IsObstacleInWay()
    {
        if (collisionDetectionRay == null)
        {
            collisionDetectionRay = new()
            {
                // TopLevel = true, // Use global transforms
                CollisionMask = 1, // Only check for obstacles
                TargetPosition = new Vector3(0, 0, 2),
            };
            _nonPlayer.AddChild(collisionDetectionRay);
        }

        // Position the ray just above the characters feet
        Vector3 offset = new(0, 0.2f, 0);
        collisionDetectionRay.GlobalPosition = _nonPlayer.GlobalPosition + offset;
        collisionDetectionRay.GlobalRotation = _nonPlayer.GlobalRotation;

        // Sweep the ray in a cone ahead of the character
        float coneAngle = 90f;
        int rayCount = (int)Math.Abs(coneAngle); // Number of rays to cast within the cone. Checking every degree for now but can be optimized for performance.
        float angleStep = coneAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float angle = -coneAngle / 2 + i * angleStep;
            collisionDetectionRay.Rotation = new Vector3(0, Mathf.DegToRad(angle), 0);
            collisionDetectionRay.ForceRaycastUpdate();

            if (collisionDetectionRay.IsColliding())
            {
                collisionDetectionRay.Rotation = new Vector3(0, 0, 0);
                return true;
            }
        }

        collisionDetectionRay.Rotation = new Vector3(0, 0, 0);
        return false;
    }

}
