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

		MovementController.Initialize(this);
	}


    public override void _Process(double delta)
    {
        base._Process(delta);

		MovementController.OnProcess(delta);
    }

	public void SetHitboxScaleY(float value)
	{
		if (Hitbox == null) return;
		if (value == Hitbox.Scale.Y) return;

		Vector3 newScale = Hitbox.Scale;
		newScale.Y = value;

		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(Hitbox, "scale", newScale, 0.025f);
		tween.Play();
	}

}
