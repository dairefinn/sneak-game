namespace SneakGame;

using System.Collections.Generic;
using Godot;

public partial class NonPlayer : CharacterBody3D
{

	[Export] public Character Character { get; set; }

	[Export] public CollisionShape3D Hitbox { get; set; }
	[Export] public Area3D DetectionArea { get; set; }
	[Export] public NonPlayerBrain Brain { get; set; }


	public override void _Ready()
	{
		base._Ready();

		Brain?.Initialize(this);

		if (DetectionArea != null)
		{
			DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
			DetectionArea.BodyExited += OnDetectionAreaBodyExited;
		}
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
    }


	private void OnDetectionAreaBodyEntered(Node body)
	{
		Brain?.AddDetectedBody(body);
	}

	private void OnDetectionAreaBodyExited(Node body)
	{
		Brain?.RemoveDetectedBody(body);
	}

}
