namespace SneakGame;

using Godot;

[GlobalClass]
public partial class Character : Resource
{

    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Texture2D Portrait { get; set; }

    [Export] public CharacterAttributes Attributes { get; set; }
    [Export] public CharacterStats Stats { get; set; }
    [Export] public CharacterRelations Relations { get; set; }
    [Export] public ItemContainer Inventory { get; set; }

    public Character CreateInstance()
    {
        Character instance = Duplicate() as Character;
        instance.Name = Name;
        instance.Description = Description;
        instance.Portrait = Portrait;

        instance.Attributes = Attributes.CreateInstance();
        instance.Stats = Stats.CreateInstance();
        instance.Relations = Relations.CreateInstance();
        instance.Inventory = Inventory.CreateInstance();

        return instance;
    }

}
