namespace SneakGame;

using Godot;

public partial class NpcActionFollow : NonPlayerAction
{

    [Export] public float FollowSpeed = 10;

    public override bool Execute(NonPlayerBrain owner, double delta)
    {
        bool success = base.Execute(owner, delta);
        if (!success) return false;

        if ((owner.TargetPosition - owner.NonPlayer.GlobalTransform.Origin).Length() <= 5)
        {
            return false;
        }

        owner.MoveToTargetNode(FollowSpeed);

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
