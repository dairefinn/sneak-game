namespace SneakGame;

using Godot;

[GlobalClass]
public partial class CameraSettings : Resource
{

	[ExportGroup("Positioning")]
	[Export] public Vector3 FocusOffset = Vector3.Zero;
	[Export] public float Distance = 0.0f;

	[ExportGroup("Mouse input")]
	[Export] public bool RotateWithMouseHorizontal { get; set; } = true;
	[Export] public bool RotateWithMouseVertical { get; set; } = true;
	[Export] public bool scrollToZoom { get; set; } = true;

}
