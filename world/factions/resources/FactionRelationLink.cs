namespace SneakGame;

using Godot;

[GlobalClass]
public partial class FactionRelationLink : Resource
{

    [Export] public Faction Faction1 { get; set; }
    [Export] public Faction Faction2 { get; set; }
    [Export] public float Affinity { get; set; } = 0;

    public override string ToString()
    {
        if (Faction1 == null || Faction2 == null)
        {
            return base.ToString();
        }
    
        return $"{Faction1.Name} - {Faction2.Name} ({Affinity})";
    }

}
