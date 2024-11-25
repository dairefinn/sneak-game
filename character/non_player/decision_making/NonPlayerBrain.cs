namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	[Export] public Array<NonPlayerAction> PossibleActions { get; set; } = new Array<NonPlayerAction>();
	[Export] public NonPlayerAction CurrentAction { get; set; } = null;
	[Export] public Array<Node3D> DetectedBodies = new();


	public NonPlayer NonPlayer;


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
		CurrentAction?.Execute(this, delta);
	}

	public void ClearAction()
	{
		CurrentAction = null;
		NonPlayer.MovementContoller.ClearTargets();
	}

	// ENTITY DETECTION

	public void AddDetectedBody(Node3D body)
	{
		if (body is NonPlayer npc && npc == NonPlayer) return; // Don't detect self

		DetectedBodies.Add(body);

		// // TODO: Used for testing. Should be replaced with a more complex decision making system. Will follow any players it sees
		if (body is Player player)
		{
            foreach (var action in PossibleActions)
            {
                if (action is NpcActionFollow followAction)
                {
                    CurrentAction = followAction;
                    return;
                }
				// if (action is NpcActionPatrol patrolAction)
				// {
				// 	CurrentAction = patrolAction;
				// 	return;
				// }
            }
		}

		// // TODO: Used for testing. Should be replaced with a more complex decision making system. Will attack any NPCs it sees
		// if (body is NonPlayer nonPlayer)
		// {
		// 	foreach (var action in PossibleActions)
		// 	{
		// 		if (action is NpcActionAttack attackAction)
		// 		{
		// 			CurrentAction = attackAction;
		// 			return;
		// 		}
		// 	}
		// }
	}

	public void RemoveDetectedBody(Node3D body)
	{
		DetectedBodies.Remove(body);

		// // TODO: Used for testing. Should be replaced with a more complex decision making system. Will start patrolling if it loses sight of a player
		if (body is Player player)
		{
			foreach (var action in PossibleActions)
			{				
				if (action is NpcActionPatrol patrolAction)
				{
					CurrentAction = patrolAction;
					return;
				}
			}
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

	public T GetFirstDetectedBody<T>() where T : Node3D
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
