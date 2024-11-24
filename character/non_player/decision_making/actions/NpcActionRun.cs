namespace SneakGame;

using Godot;

public partial class NpcActionRun : NonPlayerAction
{

    [Export] public float RunSpeed = 20;

    private double timeWithoutMoving = 0;
    private Vector3? lastPosition = null;

    public override bool Execute(NonPlayerBrain owner, double delta)
    {
        bool success = base.Execute(owner, delta);
        if (!success) return false;

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

        if (owner._detectedBodies.Count > 0)
        {
            owner.NonPlayer.MovementContoller.TargetPosition = owner._detectedBodies[0].GlobalTransform.Origin;
        }

        owner.NonPlayer.MovementContoller.MovementSpeed = RunSpeed;

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
