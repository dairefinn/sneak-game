namespace SneakGame;

using Godot;

public partial class CameraController : Node3D
{

	[Export] public Camera3D MainCamera { get; set; } = null;
	[Export] public Node3D FocusedEntity { get; set; } = null;
	[Export] public CameraSettings CameraSettings { get; set; }
	[Export] public bool RotateToFocusedEntity { get; set; } = true;


	private SpringArm3D SpringArm = null;
	private Vector3 FocusPoint = new(0, 0, 0);


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SpringArm = GetChild<SpringArm3D>(0);
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FocusCamera(RotateToFocusedEntity);

		// TODO: Remove when done testing or pause menu is implemented
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			GetTree().Quit();
		}

		if (Input.IsActionPressed("toggle_camera_lock"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		else
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

		if (Input.MouseMode == Input.MouseModeEnum.Visible) return;

		if (@event is InputEventMouseMotion mouseMotion)
		{
			if (mouseMotion.Relative.IsZeroApprox()) return;

			ApplyMouseMovementHorizontal(mouseMotion);
			ApplyMouseMovementVertical(mouseMotion);
			if (MainCamera != null)
			{
				Rotation = new Vector3(Rotation.X, Rotation.Y, 0);
			}
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			ApplyMouseZoom(mouseButton);
		}
    }


	public void FocusCamera(bool rotateToEntity = true)
	{
		if (MainCamera == null) return;

		// If we have a focused entity, focus the camera on it
		if (FocusedEntity != null)
		{
			FocusPoint = FocusedEntity.GlobalPosition;
			FocusPoint += CameraSettings.FocusOffset;
		}

		Vector3 newPosition = FocusPoint;

		// Apply the camera distance
		// newPosition += GlobalTransform.Basis.Z * CameraSettings.Distance;

		// Apply the camera rotation to the target entity
		if (FocusedEntity != null && rotateToEntity)
		{
			Vector3 newRotation = FocusedEntity.Rotation;
			newRotation.Y = Rotation.Y + Mathf.Pi;
			FocusedEntity.Rotation = newRotation;
		}

		Position = newPosition;
	}

	// This will rotate the camera around the focus point at the desired distance
	public void ApplyMouseMovementHorizontal(InputEventMouseMotion mouseMotion)
	{
		if (MainCamera == null) return;
		if (!CameraSettings.RotateWithMouseHorizontal) return;

		float rotateAmount = mouseMotion.Relative.X;
		float rotateAmountRadians = Mathf.DegToRad(rotateAmount);

		RotateObjectLocal(Vector3.Down, rotateAmountRadians);
	}

	public void ApplyMouseMovementVertical(InputEventMouseMotion mouseMotion)
	{
		if (MainCamera == null) return;
		if (!CameraSettings.RotateWithMouseVertical) return;

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

	public void ApplyMouseZoom(InputEventMouseButton mouseMotion)
	{
		if (!CameraSettings.scrollToZoom) return;

		if (mouseMotion.IsActionPressed("zoom_in"))
		{
			SpringArm.SpringLength -= 0.1f;
		}

		if (mouseMotion.IsActionPressed("zoom_out"))
		{
			SpringArm.SpringLength += 0.1f;
		}
	}
}
