namespace SneakGame;

using Godot;

[GlobalClass]
public partial class CharacterStats : Resource
{

    [Export] public int CurrentHealth { get; set; } = 100;
    [Export] public int MaxHealth { get; set; } = 100;

    [Export] public int CurrentStamina { get; set; } = 100;
    [Export] public int MaxStamina { get; set; } = 100;

    [Export] public CharacterAttributes Attributes { get; set; }

}
