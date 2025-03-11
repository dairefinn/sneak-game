namespace SneakGame;

using Godot;

public partial class Settings : Node
{

	private static Settings Instance;

	private Settings()
	{
		Instance = this;
	}

	public static Settings GetInstance()
	{
		if (Instance == null)
		{
			Instance = new Settings();
		}

		return Instance;
	}

	// Player related Settings

	public bool DebugPathfinding = true;

}
