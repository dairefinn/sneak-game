namespace SneakGame;

using Godot;

public partial class LevelHandler : Node
{

	[Export] public bool Enabled { get; set; } = true;
	[Export] public PackedScene DefaultScene { get; set; }

	public override void _Ready()
	{
		base._Ready();

		if (!Enabled) return;
		GD.Print("LevelHandler initialized");
		ChangeScene(DefaultScene);
	}

	public void ChangeScene(PackedScene scene)
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}

		Node newScene = scene.Instantiate();
		AddChild(newScene);
	}

}
