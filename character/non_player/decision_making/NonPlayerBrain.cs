namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	[Export] public Array<NonPlayerAction> PossibleActions { get; set; } = new Array<NonPlayerAction>();
	[Export] public NonPlayerAction CurrentAction { get; set; } = null;

    [ExportGroup("Conditional properties")]
    [Export] public Vector3 TargetPosition { get; set; } = Vector3.Zero;
    [Export] public Node3D TargetNode { get; set; } = null;

	public NonPlayer NonPlayer;
	public readonly Array<Node> _detectedBodies = new();

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
		TargetPosition = Vector3.Zero;
		TargetNode = null;
	}

    public bool MoveToTargetPoint(float movementSpeed = 1)
    {
        // Calculate the velocity towards the target position
        Vector3 velocity = TargetPosition - NonPlayer.GlobalTransform.Origin;

        // Cap the velocity to WalkSpeed
        if (velocity.Length() > movementSpeed)
        {
            velocity = velocity.Normalized() * movementSpeed;
        }

        NonPlayer.Velocity = velocity;

        // Face the target position if not already facing it
        if (NonPlayer.Velocity.Length() > 0.1f)
        {
            Vector3 direction = NonPlayer.Position - TargetPosition;
            NonPlayer.LookAt(NonPlayer.Position + direction, Vector3.Up);
        }

        // Apply movement
        NonPlayer.MoveAndSlide();

        return true;
    }

    public bool MoveToTargetNode(float movementSpeed = 1)
    {
        // Calculate the velocity towards the target position
        Vector3 velocity = TargetNode.GlobalTransform.Origin - NonPlayer.GlobalTransform.Origin;

        // Cap the velocity to WalkSpeed
        if (velocity.Length() > movementSpeed)
        {
            velocity = velocity.Normalized() * movementSpeed;
        }

        NonPlayer.Velocity = velocity;

        // Face the target position if not already facing it
        if (NonPlayer.Velocity.Length() > 0.1f)
        {
            Vector3 direction = NonPlayer.Position - TargetNode.GlobalTransform.Origin;
            NonPlayer.LookAt(NonPlayer.Position + direction, Vector3.Up);
        }

        // Apply movement
        NonPlayer.MoveAndSlide();

        return true;
    }

    public void TargetNewRandomPositionOnMap()
    {
        // Get a random point between -50 and 50 on the x and z axis
        TargetPosition = new Vector3(
            GD.RandRange(-50, 50),
            0,
            GD.RandRange(-50, 50)
        );
        // GD.Print("New target position: " + owner.TargetPosition);
    }

	// ENTITY DETECTION

	public void AddDetectedBody(Node body)
	{
		GD.Print("Detected body: " + body.Name);
		_detectedBodies.Add(body);

		if (body is Player player)
		{
            GD.Print("Player detected, following player");
            foreach (var action in PossibleActions)
            {
                GD.Print("Checking action: " + action);
                if (action is NpcActionFollow followAction)
                {
                    GD.Print("Found follow action");
                    TargetNode = player;
                    CurrentAction = followAction;
                    return;
                }
            }
		}
	}

	public void RemoveDetectedBody(Node body)
	{
		GD.Print("Lost body: " + body.Name);
		_detectedBodies.Remove(body);
	}

	public void ClearDetectedBodies()
	{
		_detectedBodies.Clear();
	}

	public bool IsBodyDetected(Node body)
	{
		return _detectedBodies.Contains(body);
	}

}
