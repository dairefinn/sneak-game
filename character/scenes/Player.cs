namespace SneakGame;

using Godot;

public partial class Player : CharacterBody3D
{

	[Export] public PlayerMovementController MovementController { get; set; }
	[Export] public CharacterStats Stats { get; set; }


	public CollisionShape3D Hitbox { get; set; }


	public override void _Ready()
	{
		base._Ready();

		Hitbox = GetNode<CollisionShape3D>("Hitbox");
	}


    public override void _Process(double delta)
    {
        base._Process(delta);

		MovementController.OnProcess(this, delta);
    }

}
