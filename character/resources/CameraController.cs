namespace SneakGame;

using Godot;

public partial class CameraController : Camera3D
{
	
	public static CameraController Instance { get; private set; }

	[ExportGroup("Positioning")]
	[Export] public Node3D FocusedEntity { get; set; } = null;
	[Export] private Vector3 FocusOffset = Vector3.Zero;
	[Export] private float Distance = 0.0f;

	[ExportGroup("Mouse input")]
	[Export] public bool RotateWithMouseHorizontal { get; set; } = true;
	[Export] public bool RotateWithMouseVertical { get; set; } = true;
	[Export] public bool scrollToZoom { get; set; } = true;


	private Vector3 FocusPoint = new(0, 0, 0);


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FocusCamera();

		// TODO: Remove when done testing or pause menu is implemented
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			GetTree().Quit();
		}
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

		if (@event is InputEventMouseMotion mouseMotion)
		{
			if (mouseMotion.Relative.IsZeroApprox()) return;

			ApplyMouseMovementHorizontal(mouseMotion);
			ApplyMouseMovementVertical(mouseMotion);
			Rotation = new Vector3(Rotation.X, Rotation.Y, 0);
		}

		if (scrollToZoom)
		{
			if (@event.IsActionPressed("zoom_in"))
			{
				Distance -= 0.1f;
			}

			if (@event.IsActionPressed("zoom_out"))
			{
				Distance += 0.1f;
			}
		}
    }


	public void FocusCamera()
	{
		// If we have a focused entity, focus the camera on it
		if (FocusedEntity != null)
		{
			FocusPoint = FocusedEntity.GlobalPosition;
			FocusPoint += FocusOffset;
		}

		Vector3 newPosition = FocusPoint;

		// Apply the camera distance
		newPosition += GlobalTransform.Basis.Z * Distance;

		Position = newPosition;
	}

	// This will rotate the camera around the focus point at the desired distance
	public void ApplyMouseMovementHorizontal(InputEventMouseMotion mouseMotion)
	{
		if (!RotateWithMouseHorizontal) return;

		float rotateAmount = mouseMotion.Relative.X;
		float rotateAmountRadians = Mathf.DegToRad(rotateAmount);

		RotateObjectLocal(Vector3.Down, rotateAmountRadians);
	}

	public void ApplyMouseMovementVertical(InputEventMouseMotion mouseMotion)
	{
		if (!RotateWithMouseVertical) return;

		float ROTATION_LIMIT_DEGREES = 45;

		float rotateAmount = mouseMotion.Relative.Y;
		float rotateAmountRadians = Mathf.DegToRad(rotateAmount);
		float limitToRadians = Mathf.DegToRad(ROTATION_LIMIT_DEGREES);

		float newRotation = Rotation.X + rotateAmountRadians;

		// Camera down
		if (rotateAmount > 0) {
			bool newRotationIsLessThanLimit = newRotation <= -limitToRadians;
			if (!newRotationIsLessThanLimit) {
				RotateObjectLocal(Vector3.Left, rotateAmountRadians);
			}

			return;
		}

		// Camera up
		if (rotateAmount < 0) {
			bool newRotationIsGreaterThanLimit = newRotation >= limitToRadians;
			if (!newRotationIsGreaterThanLimit) {
				RotateObjectLocal(Vector3.Left, rotateAmountRadians);
			}
			return;
		}
	}
}
