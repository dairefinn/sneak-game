namespace SneakGame;

using Godot;


[GlobalClass]
public partial class Faction : Resource
{

	[ExportGroup("General")]
	[Export] public string Name { get; set; } = "Faction name";
	[Export] public Color Color { get; set; } = Colors.White;
	[Export] public Texture2D Icon { get; set; }

    public Faction CreateInstance()
    {
        Faction instance = Duplicate() as Faction;
        
		instance.Name = Name;
		instance.Color = Color;
		instance.Icon = Icon;

        return instance;
    }

}
