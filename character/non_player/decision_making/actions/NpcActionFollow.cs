namespace SneakGame;

using Godot;

public partial class NpcActionFollow : NonPlayerAction
{

    [Export] public float FollowSpeed = 10;

    private Node3D _target;

    public override bool Execute(NonPlayerBrain owner, double delta)
    {
        bool success = base.Execute(owner, delta);
        if (!success) return false;

        Node3D newTarget = owner.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            _target = newTarget;
        }

        if (_target == null) return false;

        
        owner.NonPlayer.MovementContoller.TargetPosition = _target.GlobalTransform.Origin;
        owner.NonPlayer.MovementContoller.MovementSpeed = FollowSpeed;

        return true;
    }

}
