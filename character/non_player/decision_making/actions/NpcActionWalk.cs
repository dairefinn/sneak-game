namespace SneakGame;

using Godot;

public partial class NpcActionWalk : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.WALK;

    [Export] public float MovementSpeed = 10;
    [Export] public Vector3 TargetPosition;
    [Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }

    // private double timeWithoutMoving = 0;
    // private Vector3? lastPosition = null;

    public override void OnProcess(double delta)
    {
        if (!CanExecute()) return;

        Node3D newTarget = DetectionHandler.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            Brain.NonPlayer.MovementContoller.TargetPosition = newTarget.GlobalTransform.Origin;
        }

        Brain.NonPlayer.MovementContoller.Speed = MovementSpeed;
        Brain.NonPlayer.MovementContoller.TargetPosition = TargetPosition;

        return;
    }

}
