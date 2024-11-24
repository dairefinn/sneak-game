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

}
