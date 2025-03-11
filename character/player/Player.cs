namespace SneakGame;

using Godot;

public partial class Player : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public PlayerMovementController MovementController { get; set; }
	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public AnimationTree AnimationTree { get; set; }

	public override void _Ready()
	{
		base._Ready();

		if (MovementController != null)
		{
			MovementController.Player = this;
		}

		Events.GetInstance().PlayerAddHealth += OnPlayerAddHealth;
	}

	private void OnPlayerAddHealth(int amount)
	{
		Character.Stats.CurrentHealth += amount;
	}

}
