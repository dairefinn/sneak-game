namespace SneakGame;

using Godot;

public partial class NonPlayer : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public NonPlayerBrain Brain { get; set; }
	[Export] public NonPlayerMovementController MovementContoller { get; set; }
	[Export] public NonPlayerDetectionHandler DetectionHandler { get; set; }


	public override void _Ready()
	{
		base._Ready();

		MovementContoller?.Initialize(this);
		DetectionHandler?.Initialize(this);
		Brain?.Initialize(this);

	}

}
