namespace DeckBuilder;

using Godot;

public partial class Events : Node
{

	private static Events Instance;

	private Events()
	{
		Instance = this;
	}

	public static Events GetInstance()
	{
		if (Instance == null)
		{
			Instance = new Events();
		}

		return Instance;
	}

	// Player related events

	[Signal] public delegate void PlayerAddHealthEventHandler(int amount);

}
