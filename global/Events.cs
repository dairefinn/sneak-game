namespace DeckBuilder;

using Godot;

public partial class Events : Node
{

	public static Events Instance;

	public Events()
	{
		Instance = this;
	}

	// Player related events

	[Signal] public delegate void PlayerAddHealthEventHandler(int amount);

}
