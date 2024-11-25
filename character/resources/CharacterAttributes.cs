namespace SneakGame;

using Godot;

[GlobalClass]
public partial class CharacterAttributes : Resource
{

    [Export] public int Strength { get; set; } = 1;
    [Export] public int Dexterity { get; set; } = 1;
    [Export] public int Intelligence { get; set; } = 1;
    [Export] public int Vitality { get; set; } = 1;

    public CharacterAttributes CreateInstance()
    {
        CharacterAttributes instance = Duplicate() as CharacterAttributes;

        instance.Strength = Strength;
        instance.Dexterity = Dexterity;
        instance.Intelligence = Intelligence;
        instance.Vitality = Vitality;

        return instance;
    }

}