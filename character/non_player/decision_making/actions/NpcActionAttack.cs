namespace SneakGame;

using Godot;

public partial class NpcActionAttack : NonPlayerAction
{

	private Node3D _target;
	private SceneTreeTimer _detectionTimer;

    public override bool Execute(NonPlayerBrain owner, double delta)
	{
		Node3D newTarget = owner.GetFirstDetectedBody<NonPlayer>();
        if (newTarget != null)
        {
            _target = newTarget;
			_detectionTimer = owner.GetTree().CreateTimer(2.0f, false);
        }

		// If the target is not detected for 2 seconds, clear the target
		if (_detectionTimer == null || _detectionTimer.TimeLeft <= 0)
		{
			_target = null;
		}

		if (_target == null)
		{
			owner.ClearAction();
			return false;
		}

		return true;
	}

}
