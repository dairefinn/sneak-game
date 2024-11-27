namespace SneakGame;

using Godot;

public partial class NpcActionFollow : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.FOLLOW;

    [Export] public float MovementSpeed = 3;

    [Export] public NodePath TargetPath;


    private Node3D _target;


    public override void OnProcess(double delta)
    {
        if (!CanExecute()) return;

        Node3D newTarget = Brain.GetFirstDetectedBody<Player>();
        if (newTarget != null)
        {
            _target = newTarget;
        }

        if (_target == null) return;

        
        Brain.NonPlayer.MovementContoller.TargetPosition = _target.GlobalTransform.Origin;
        Brain.NonPlayer.MovementContoller.MovementSpeed = MovementSpeed;

        return;
    }

}
