namespace SneakGame;

using Godot;

/// <summary>
/// Do nothing. Stand still.
/// </summary>
public partial class NpcActionIdle : NonPlayerAction
{

	public override Type ActionType { get; set; } = Type.IDLE;


	public override void OnProcess(double delta)
	{
		return;
    }

}
