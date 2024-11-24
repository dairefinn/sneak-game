namespace SneakGame;

using Godot;
using Godot.Collections;

public partial class NonPlayerBrain : Node
{

	[Export] public Array<NonPlayerAction> PossibleActions { get; set; } = new Array<NonPlayerAction>();
	[Export] public NonPlayerAction CurrentAction { get; set; } = null;

    [ExportGroup("Conditional properties")]
    [Export] public Vector3 TargetPosition { get; set; } = Vector3.Zero;
    [Export] public Node TargetNode { get; set; } = null;

	public NonPlayer NonPlayer;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		TryPerformCurrentAction();
	}

	public void Initialize(NonPlayer nonPlayer)
	{
		NonPlayer = nonPlayer;
	}

	public void TryPerformCurrentAction()
	{
		if (CurrentAction == null) return;

		CurrentAction.Execute(this);
	}

}
