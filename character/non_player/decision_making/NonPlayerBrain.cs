#nullable enable

namespace SneakGame;

using System.Linq;
using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	protected Dictionary<NonPlayerAction.Type, NonPlayerAction> Actions = new();

	[Export] public NonPlayerAction.Type DefaultActionType = NonPlayerAction.Type.PATROL;
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
			// TODO: Register dependencies depending on action type (Movement controller, detection handler, etc)
		}

		OnTransitionRequested(null, DefaultActionType);
	}

	public void ChangeToAction(NonPlayerAction.Type actionType)
	{
		if (CurrentAction != null)
		{
			CurrentAction.ChangeToAction(actionType);
		}
		else
		{
			OnTransitionRequested(null, actionType);
		}
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

}
