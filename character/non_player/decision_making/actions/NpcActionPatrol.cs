namespace SneakGame;

using Godot;
using Godot.Collections;

/// <summary>
/// The NPC will patrol between points in the Path array.
/// </summary>
public partial class NpcActionPatrol : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.PATROL;

    [Export] public float MovementSpeed = 3;
    [Export] public float TargetReachedThreshold = 1;
    [Export] public Array<Vector3> Path = new();
    [Export] public NonPlayer NonPlayer { get; set; }
    [Export] public NonPlayerMovementController MovementController { get; set; }


    public Vector3? CurrentTarget;

    private MeshInstance3D _pathMesh;
    private bool patrolStarted = false;


    public override void OnProcess(double delta)
    {
        if (!CanExecute()) return;

        if (CurrentTarget == null)
        {
            GD.Print("CurrentTarget is null. Looking for nearest point on path.");
            CurrentTarget = GetNearestPointOnPath(NonPlayer.GlobalTransform.Origin);
        }

        if(CurrentTarget == null) return;

        Vector3 currentTargetUsing = (Vector3) CurrentTarget;

        if (MovementController.IsNearPosition(currentTargetUsing, TargetReachedThreshold))
        {
            CurrentTarget = GetNextTargetOnPath(currentTargetUsing);
        }

        if (!patrolStarted)
        {
            MovementController.TargetPosition = currentTargetUsing;
            MovementController.StopThreshold = TargetReachedThreshold;
            MovementController.MovementSpeed = MovementSpeed;
        }
       
        DrawDebug();

        return;
    }

    public override void Exit()
    {
        CurrentTarget = null;
    }

    /// <summary>
    /// Returns the next target on the path. If at the end of the path, returns the first target.
    /// </summary>
    /// <param name="currentTarget"></param>
    /// <returns>The next target on the path</returns>
    private Vector3 GetNextTargetOnPath(Vector3 currentTarget)
    {
        int currentIndex = Path.IndexOf(currentTarget);
        if (currentIndex == -1) return Path[0];

        int nextIndex = currentIndex + 1;
        if (nextIndex >= Path.Count) nextIndex = 0;

        return Path[nextIndex];
    }
    
    private Vector3 GetNearestPointOnPath(Vector3 currentPosition)
    {
        Vector3 nearestPoint = Path[0];
        float nearestDistance = currentPosition.DistanceTo(nearestPoint);
        foreach (var point in Path)
        {
            float distance = currentPosition.DistanceTo(point);
            if (distance < nearestDistance)
            {
                nearestPoint = point;
                nearestDistance = distance;
            }
        }

        return nearestPoint;
    }

    private void DrawDebug()
    {
        if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        Vector3 meshOffset = new(0, 1f, 0);
        if (_pathMesh == null)
        {
            _pathMesh = new();
            GetTree().CurrentScene.AddChild(_pathMesh);
        }
        
        // Draw the patrol path
        ImmediateMesh pathImmediateMesh = new();
        pathImmediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
        foreach (var point in Path)
        {
            Vector3 pointRaised = point;
            pointRaised += meshOffset;
            pathImmediateMesh.SurfaceAddVertex(pointRaised);
        }

        if (Path.Count > 0)
        {
            Vector3 firstPointRaised = Path[0];
            firstPointRaised += meshOffset;
            pathImmediateMesh.SurfaceAddVertex(firstPointRaised);
        }

        pathImmediateMesh.SurfaceEnd();

        // Draw circles around each point to indicate how close the NPC needs to be to consider it reached
        Array<ImmediateMesh> thresholdCircles = new();
        foreach (var point in Path)
        {
            ImmediateMesh circleMesh = new();
            circleMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);
            Vector3? firstPoint = null;
            for (int i = 0; i < 360; i += 45) // The increment here will determine the number of points in the circle (90 = 4 points, 45 = 8 points, etc.)
            {
                float angle = Mathf.DegToRad(i);
                float x = Mathf.Cos(angle) * TargetReachedThreshold;
                float z = Mathf.Sin(angle) * TargetReachedThreshold;
                Vector3 circlePoint = new Vector3(x, 0, z) + point;
                circlePoint += meshOffset;
                if (firstPoint == null) firstPoint = circlePoint;
                circleMesh.SurfaceAddVertex(circlePoint);
            }
            if (firstPoint != null) circleMesh.SurfaceAddVertex(firstPoint.Value);
            circleMesh.SurfaceEnd();
            thresholdCircles.Add(circleMesh);
        }

        // Combine the meshes
        ArrayMesh combinedMesh = new();
        combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, pathImmediateMesh.SurfaceGetArrays(0));
        combinedMesh.SurfaceSetMaterial(0, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Blue });
        foreach (var circleMesh in thresholdCircles)
        {
            combinedMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.LineStrip, circleMesh.SurfaceGetArrays(0));
            combinedMesh.SurfaceSetMaterial(combinedMesh.GetSurfaceCount() - 1, new StandardMaterial3D() { EmissionEnabled = true, AlbedoColor = Colors.Red });
        }

        _pathMesh.Mesh = combinedMesh;
    }

}
