namespace SneakGame;

using Godot;

public partial class NonPlayer : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public NonPlayerMovementController MovementContoller { get; set; }
	[Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }
	[Export] public NonPlayerBrain Brain { get; set; }


	public override void _Ready()
	{
		base._Ready();

		if(MovementContoller != null)
		{
			MovementContoller.NonPlayer = this;
		}

		if (DetectionHandler != null)
		{
			DetectionHandler.NonPlayer = this;
		}

		if (Brain != null)
		{
			Brain.NonPlayer = this;
		}
	}

}
