#nullable enable

namespace SneakGame;

using System.Linq;
using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	protected Dictionary<NonPlayerAction.Type, NonPlayerAction> Actions = new();

	[Export] public NonPlayerAction.Type DefaultActionType = NonPlayerAction.Type.PATROL;
	[Export] public Array<Node3D> DetectedBodies = new();
	public NonPlayer? NonPlayer;


	private NonPlayerAction? CurrentAction;


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		CurrentAction?.OnProcess(delta);
	}

	public override void _Input(InputEvent _event)
	{
		CurrentAction?.OnInput(_event);
	}


	public void Initialize(NonPlayer nonPlayer)
	{
		NonPlayer = nonPlayer;

		foreach (NonPlayerAction action in GetChildren().OfType<NonPlayerAction>())
		{
			Actions[action.ActionType] = action;
			action.Brain = this;
			action.TransitionRequested += OnTransitionRequested;
		}

		OnTransitionRequested(null, DefaultActionType);
	}

	public void OnTransitionRequested(NonPlayerAction? from, NonPlayerAction.Type to)
	{
		if (from != CurrentAction) return;

		NonPlayerAction newState = Actions[to];
		if (newState == null) return;

		GD.Print($"Transitioning from {from?.ActionType} to {to}");

		CurrentAction?.Exit();

		newState.Enter();
		CurrentAction = newState;
		newState.PostEnter();
	}

	// ENTITY DETECTION
	// TODO: Move to a separate handler and use signals to consume
	// TODO: Add some timers here so that it's less jarring. eg Player is only "Detected" when they're in the vision cone for a certain amount of time and then "Lost" when they're out of the vision cone for a certain amount of time

	public void AddDetectedBody(Node3D body)
	{
		if (body is NonPlayer npc && npc == NonPlayer) return; // Don't detect self

		DetectedBodies.Add(body);

		// // TODO: Used for testing. Should be replaced with a more complex decision making system. Will follow any players it sees
		if (body is Player player)
		{
			CurrentAction?.ChangeToAction(NonPlayerAction.Type.FOLLOW);
		}

		// TODO: Used for testing. Should be replaced with a more complex decision making system. Will attack any NPCs it sees
		// if (body is NonPlayer nonPlayer)
		// {
		// 	CurrentAction.EmitSignal(NonPlayerAction.SignalName.TransitionRequested, (int)NonPlayerAction.Type.ATTACK);
		// }
	}

	public void RemoveDetectedBody(Node3D body)
	{
		DetectedBodies.Remove(body);

		// // TODO: Used for testing. Should be replaced with a more complex decision making system. Will start patrolling if it loses sight of a player
		if (body is Player player)
		{
			CurrentAction?.ChangeToAction(NonPlayerAction.Type.PATROL);
		}
	}

	public void ClearDetectedBodies()
	{
		DetectedBodies.Clear();
	}

	public bool IsBodyDetected(Node3D body)
	{
		return DetectedBodies.Contains(body);
	}

	public T? GetFirstDetectedBody<T>() where T : Node3D
	{
		if (DetectedBodies.Count == 0) return null;

		foreach (Node3D body in DetectedBodies)
		{
			if (body is T tBody)
			{
				return tBody;
			}
		}

		return null;
	}

}
