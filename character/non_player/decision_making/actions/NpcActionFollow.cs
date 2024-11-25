namespace SneakGame;

using Godot;

public partial class NpcActionFollow : NonPlayerAction
{

    [Export] public float MovementSpeed = 10;

    [Export] public NodePath TargetPath;

    public Node3D Target;

    public override bool Execute(NonPlayerBrain owner, double delta)
    {
        bool success = base.Execute(owner, delta);
        if (!success) return false;

        Node3D newTarget = owner.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            Target = newTarget;
        }

        if (Target == null) return false;

        
        owner.NonPlayer.MovementContoller.TargetPosition = Target.GlobalTransform.Origin;
        owner.NonPlayer.MovementContoller.MovementSpeed = MovementSpeed;

        return true;
    }

}
