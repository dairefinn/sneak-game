namespace SneakGame;

using DeckBuilder;
using Godot;

public partial class Player : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public PlayerMovementController MovementController { get; set; }
	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }


	public override void _Ready()
	{
		base._Ready();

		MovementController?.Initialize(this, Hitbox, CameraController.Instance, AnimationPlayer);

		Events.GetInstance().PlayerAddHealth += OnPlayerAddHealth;

	}

	private void OnPlayerAddHealth(int amount)
	{
		Character.Stats.CurrentHealth += amount;
	}

}
