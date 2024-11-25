namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NpcActionPatrol : NonPlayerAction
{

    [Export] public float MovementSpeed = 10;
    [Export] public float TargetReachedThreshold = 5;
    [Export] public Array<Vector3> Path = new();


    public Vector3 CurrentTarget;

    private MeshInstance3D _pathMesh;


    public override bool Execute(NonPlayerBrain owner, double delta)
    {
        bool success = base.Execute(owner, delta);
        if (!success) return false;

        if (owner.NonPlayer.MovementContoller.IsNearPosition(CurrentTarget, TargetReachedThreshold))
        {
            CurrentTarget = GetNextTargetOnPath(CurrentTarget);
        }
       
        owner.NonPlayer.MovementContoller.TargetPosition = CurrentTarget;
        owner.NonPlayer.MovementContoller.MovementSpeed = MovementSpeed;

        DrawDebug(owner);

        return true;
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

    private void DrawDebug(NonPlayerBrain owner)
    {
        if (!NavigationServer3D.GetDebugEnabled()) return; // Only show if Show Navigation debug is enabled

        // Add the mesh container to the scene
        Vector3 meshOffset = new(0, 1f, 0);
        if (_pathMesh == null)
        {
            _pathMesh = new();
            owner.GetTree().CurrentScene.AddChild(_pathMesh);
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
            for (int i = 0; i < 360; i += 10)
            {
                float angle = Mathf.DegToRad(i);
                float x = Mathf.Cos(angle) * TargetReachedThreshold;
                float z = Mathf.Sin(angle) * TargetReachedThreshold;
                Vector3 circlePoint = new Vector3(x, 0, z) + point;
                circlePoint += meshOffset;
                circleMesh.SurfaceAddVertex(circlePoint);
            }
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
