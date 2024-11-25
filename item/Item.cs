namespace SneakGame;

using Godot;

[GlobalClass]
public partial class Item : Resource
{

    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }

    public Item CreateInstance()
    {
        Item instance = Duplicate() as Item;
		
        instance.Name = Name;
        instance.Description = Description;

        return instance;
    }

}
