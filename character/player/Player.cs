namespace SneakGame;

using DeckBuilder;
using Godot;

public partial class Player : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public PlayerMovementController MovementController { get; set; }
	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public AnimationTree AnimationTree { get; set; }
	[Export] public CameraController CameraController { get; set; }


	public override void _Ready()
	{
		base._Ready();

		MovementController?.Initialize(this, Hitbox, CameraController, AnimationTree);

		Events.GetInstance().PlayerAddHealth += OnPlayerAddHealth;

	}

	private void OnPlayerAddHealth(int amount)
	{
		Character.Stats.CurrentHealth += amount;
	}

}
