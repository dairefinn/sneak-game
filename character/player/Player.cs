namespace SneakGame;

using Godot;

public partial class Player : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public PlayerMovementController MovementController { get; set; }
	[Export] public CollisionShape3D Hitbox { get; set; }


	public override void _Ready()
	{
		base._Ready();

		MovementController?.Initialize(this, CameraController.Instance);
	}

}
