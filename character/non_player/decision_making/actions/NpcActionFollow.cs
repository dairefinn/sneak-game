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

        if (owner._detectedBodies.Count > 0)
        {
            GD.Print("Following detected body: " + owner._detectedBodies[0].Name);
            _target = owner._detectedBodies[0];
        }

        if (_target == null) return false;

        
        owner.NonPlayer.MovementContoller.TargetPosition = _target.GlobalTransform.Origin;
        owner.NonPlayer.MovementContoller.MovementSpeed = FollowSpeed;

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
