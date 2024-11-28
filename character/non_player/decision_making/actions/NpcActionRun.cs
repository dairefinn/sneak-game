namespace SneakGame;

using Godot;

public partial class NpcActionRun : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.RUN;

    [Export] public float MovementSpeed = 20;
    [Export] public Vector3 TargetPosition;
	[Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }

    private double _timeWithoutMoving = 0;
    // private Vector3? lastPosition = null;

    public override void OnProcess(double delta)
    {
        if (!CanExecute()) return;

        // TODO: Might make more sense to have this in the movement controller
        // if (lastPosition != null)
        // {
        //     Vector3 distanceMoved = (owner.NonPlayer.GlobalTransform.Origin - lastPosition).Value;
        //     if (distanceMoved.Length() <= 0.1f)
        //     {
        //         timeWithoutMoving += delta;
        //         if (timeWithoutMoving >= 5)
        //         {
        //             owner.NonPlayer.MovementContoller.TargetNewRandomPositionOnMap();
        //             timeWithoutMoving = 0;
        //         }
        //     }
        //     else
        //     {
        //         timeWithoutMoving = 0;
        //     }
        // }

        // lastPosition = owner.NonPlayer.GlobalTransform.Origin;

        Node3D newTarget = DetectionHandler.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            Brain.NonPlayer.MovementContoller.TargetPosition = newTarget.GlobalTransform.Origin;
        }

        Brain.NonPlayer.MovementContoller.MovementSpeed = MovementSpeed;

        return;
    }

}
