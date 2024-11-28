#nullable enable

namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NonPlayerDetectionHandler : Area3D
{

	[Export] public Array<Node3D> DetectedBodies = new();

    private NonPlayer _nonPlayer;


    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }


    public void Initialize(NonPlayer nonPlayer)
    {
        _nonPlayer = nonPlayer;
    }

    // TODO: Add some timers here so that it's less jarring. eg Player is only "Detected"
    // when they're in the vision cone for a certain amount of time and then "Lost"
    // when they're out of the vision cone for a certain amount of time
    private void OnBodyEntered(Node3D body)
    {
        if (BodyIsSelf(body)) return;

        DetectedBodies.Add(body);

        if (body is Player player)
        {
            _nonPlayer.Brain.ChangeToAction(NonPlayerAction.Type.FOLLOW);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (BodyIsSelf(body)) return;

        bool success = DetectedBodies.Remove(body);

        if (success && body is Player player)
        {
            _nonPlayer.Brain.ChangeToAction(NonPlayerAction.Type.PATROL);
        }
    }

    private bool BodyIsSelf(Node3D body)
    {
        if (body is not NonPlayer npc) return false;
        if (npc != _nonPlayer) return false;
        return true;
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
