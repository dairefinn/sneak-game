namespace SneakGame;

using Godot;

[GlobalClass]
public partial class CharacterRelations : Resource
{

    [Export] public Faction Faction { get; set; }

    public CharacterRelations CreateInstance()
    {
        CharacterRelations instance = Duplicate() as CharacterRelations;
        
        instance.Faction = Faction.CreateInstance();

        return instance;
    }

}
