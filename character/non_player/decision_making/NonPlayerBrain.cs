namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	[Export] public Array<NonPlayerAction> PossibleActions { get; set; } = new Array<NonPlayerAction>();
	[Export] public NonPlayerAction CurrentAction { get; set; } = null;

    [ExportGroup("Conditional properties")]


	public NonPlayer NonPlayer;
	public readonly Array<Node3D> _detectedBodies = new();

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		TryPerformCurrentAction(delta);
	}

	public void Initialize(NonPlayer nonPlayer)
	{
		NonPlayer = nonPlayer;
	}

	public void TryPerformCurrentAction(double delta)
	{
		if (CurrentAction == null) return;

		CurrentAction.Execute(this, delta);
	}

	public void ClearAction()
	{
		CurrentAction = null;
		NonPlayer.MovementContoller.ClearTargets();
	}

	// ENTITY DETECTION

	public void AddDetectedBody(Node3D body)
	{
		_detectedBodies.Add(body);

		if (body is Player player)
		{
            foreach (var action in PossibleActions)
            {
                if (action is NpcActionFollow followAction)
                {
                    CurrentAction = followAction;
                    return;
                }
            }
		}
	}

	public void RemoveDetectedBody(Node3D body)
	{
		_detectedBodies.Remove(body);
	}

	public void ClearDetectedBodies()
	{
		_detectedBodies.Clear();
	}

	public bool IsBodyDetected(Node3D body)
	{
		return _detectedBodies.Contains(body);
	}

}
