namespace SneakGame;

using Godot;

public partial class Globals : Node
{

	private static Globals Instance;

	private Globals()
	{
		Instance = this;
	}

	public static Globals GetInstance()
	{
		if (Instance == null)
		{
			Instance = new Globals();
		}

		return Instance;
	}

	// Player related Globals

	public Player CurrentPlayer = null;

}
