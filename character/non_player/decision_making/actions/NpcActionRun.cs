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

        if (lastPosition != null)
        {
            Vector3 distanceMoved = (owner.NonPlayer.GlobalTransform.Origin - lastPosition).Value;
            if (distanceMoved.Length() <= 0.1f)
            {
                timeWithoutMoving += delta;
                if (timeWithoutMoving >= 5)
                {
                    NpcActionUtilities.GetNewRandomPositionOnMap(owner);
                    timeWithoutMoving = 0;
                }
            }
            else
            {
                timeWithoutMoving = 0;
            }
        }

        if ((owner.TargetPosition - owner.NonPlayer.GlobalTransform.Origin).Length() <= 5 || timeWithoutMoving >= 5)
        {
            NpcActionUtilities.GetNewRandomPositionOnMap(owner);
            return false;
        }

        lastPosition = owner.NonPlayer.GlobalTransform.Origin;
        NpcActionUtilities.MoveToPoint(owner, RunSpeed);

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
