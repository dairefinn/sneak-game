namespace SneakGame;

using Godot;

/// <summary>
/// The NPC will attack the target if it is detected.
/// </summary>
public partial class NpcActionAttack : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.ATTACK;

	[Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }


	private Node3D _target;
	private SceneTreeTimer _detectionTimer;



	public override void OnProcess(double delta)
	{
		if (!CanExecute()) return;

		Node3D newTarget = DetectionHandler.GetFirstDetectedBody<NonPlayer>();
        if (newTarget != null)
        {
            _target = newTarget;
			_detectionTimer = GetTree().CreateTimer(2.0f, false);
        }

		// If the target is not detected for 2 seconds, clear the target
		if (_detectionTimer == null || _detectionTimer.TimeLeft <= 0)
		{
			_target = null;
		}

		if (_target == null)
		{
			EmitSignal(SignalName.TransitionRequested, (int)NonPlayerAction.Type.PATROL);
			return;
		}

		// TODO: Implement attack logic
    }

}
