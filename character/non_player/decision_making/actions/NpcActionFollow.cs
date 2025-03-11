namespace SneakGame;

using Godot;

/// <summary>
/// The NPC will follow the target if it is detected.
/// </summary>
public partial class NpcActionFollow : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.FOLLOW;

    [Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }
    [Export] public NonPlayerMovementController MovementContoller { get; set; }
    [Export] public float MovementSpeed = 3;


    private Node3D _target;


    public override void OnProcess(double delta)
    {
        if (!CanExecute()) return;

        Node3D newTarget = DetectionHandler.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            GD.Print("New target found: " + newTarget.Name);
            _target = newTarget;
        }

        if (_target == null) return;

        
        MovementContoller.TargetPosition = _target.GlobalTransform.Origin;
        MovementContoller.Speed = MovementSpeed;

        return;
    }

}
