namespace SneakGame;

using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ItemContainer : Resource
{

	[Export] public string Name { get; set; }
	[Export] public Array<ItemSlot> Slots { get; set; } = new Array<ItemSlot>();

    public ItemContainer CreateInstance()
    {
        ItemContainer instance = Duplicate() as ItemContainer;
		
		instance.Name = Name;
		instance.Slots = new Array<ItemSlot>();
		foreach (ItemSlot slot in Slots)
		{
			instance.Slots.Add(slot.CreateInstance());
		}

        return instance;
    }

}
