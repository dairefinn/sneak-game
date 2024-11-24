namespace SneakGame;

using Godot;

public partial class NpcActionWalk : NonPlayerAction
{

    public override bool Execute(NonPlayerBrain owner)
    {
        bool success = base.Execute(owner);
        if (!success) return false;

        if (owner.TargetPosition == owner.NonPlayer.GlobalTransform.Origin)
        {
            // owner.TargetPosition = Vector3.Zero;
            // owner.CurrentAction = null;
            return false;
        }

        // Start moving towards the target position
        owner.NonPlayer.Velocity = owner.TargetPosition - owner.NonPlayer.GlobalTransform.Origin;

        // Face the target position if not already facing it
        if (owner.NonPlayer.Velocity.Length() > 0.1f)
        {
            Vector3 direction = owner.NonPlayer.Position - owner.TargetPosition;
            owner.NonPlayer.LookAt(owner.NonPlayer.Position + direction, Vector3.Up);
        }

        // Apply movement
        owner.NonPlayer.MoveAndSlide();

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
