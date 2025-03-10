namespace SneakGame;

using System.Collections.Generic;
using Godot;

public partial class NonPlayerMovementController : Node
{

    [Export] public float MovementSpeed { get; set; } = 3;
	[Export] public int JumpVelocity = 5;
    [Export] public bool AffectedByGravity { get; set; } = true;
	[Export] public Vector3 Gravity = new Vector3(0, -9.8f, 0);
    
    // The distance from the target position at which the player will consider themselves to have reached it
    [Export] public float StopThreshold { get; set; } = 1f;
    [Export] public Vector3 TargetPosition { get; set; }

    private NonPlayer _nonPlayer;
    private MeshInstance3D _debugMesh;
	private bool _crouching = false;
	private bool _jumping = false;

    private Vector3? previousPosition;
    private SceneTreeTimer hasMovedTimer;

    private RayCast3D collisionRayCast;

    private Queue<Vector3> _vectorQueue = new();


    public void Initialize(NonPlayer nonPlayer)
    {
        _nonPlayer = nonPlayer;
        TargetPosition = _nonPlayer.GlobalTransform.Origin;
    }


    public override void _Process(double delta)
    {
        base._Process(delta);

        previousPosition = _nonPlayer.GlobalTransform.Origin;

        // Process all the vectors in the queue from the last frame
        Vector3 desiredVelocity = MergeVectorQueue();
        _nonPlayer.Velocity = desiredVelocity;

        // Face the target position if not already facing itx
        FaceTargetDirection();

        // Move the player
        bool collidedWithSomething = _nonPlayer.MoveAndSlide();

        // TODO: I used to check if the character had moved in the past X seconds and find a new path if they hadn't. Need to reimplement that.

        // Queue all the vectors we want to apply to the player in the next frame
        Vector3 physicsVector = GetPhysicsVector();
        QueueVector(physicsVector);

        Vector3 desiredMovementVector = GetDesiredMovementVector();
        QueueVector(desiredMovementVector);

        DrawDebug();
    }

    public void QueueVector(Vector3 vector)
    {
        _vectorQueue.Enqueue(vector);
    }

    public void QueueVector(Vector2 vector)
    {
        _vectorQueue.Enqueue(new Vector3(vector.X, 0, vector.Y));
    }

    /// <summary>
    /// Combine all the vectors in the queue and apply them to the player. We can then limit the player's speed by the MovementSpeed property.
    /// </summary>
    private Vector3 MergeVectorQueue()
    {
        if (_vectorQueue.Count == 0) return Vector3.Zero;

        Vector3 combinedVector = Vector3.Zero;
        while (_vectorQueue.Count > 0)
        {
            combinedVector += _vectorQueue.Dequeue();
        }

        // Limit the speed of the player
        combinedVector = combinedVector.Normalized() * MovementSpeed;

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

        Vector3 currentPosition = _nonPlayer.GlobalTransform.Origin;

        if (IsNearPosition(TargetPosition, StopThreshold)) return desiredMovement;

        // Get the normalized direction to the target position
        desiredMovement = TargetPosition - currentPosition;

        return desiredMovement;
    }

    private void FaceTargetDirection()
    {
        Vector3 direction = _nonPlayer.GlobalPosition - TargetPosition;

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
