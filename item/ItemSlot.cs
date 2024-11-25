namespace SneakGame;

using Godot;

[GlobalClass]
public partial class ItemSlot : Resource
{

    [Export] public Item Item {
        get => _item;
        set => SetItem(value);
    }
    private Item _item;
    [Export] public int Quantity {
        get => _quantity;
        set => SetQuantity(value);
    }
    private int _quantity;


    public void SetItem(Item item)
    {
        _item = item;

        if (_item == null)
        {
            _quantity = 0;
        }
    }

    public void SetQuantity(int quantity)
    {
        _quantity = quantity;

        if (_quantity < 1)
        {
            _quantity = 0;
            _item = null;
        }
    }


    public override string ToString()
    {
        if (Item == null)
        {
            return "Empty slot";
        }

        if (Quantity == 0)
        {
            return Item.Name;
        }

        if (Quantity == 1)
        {
            return Item.Name;
        }

        return $"{Item.Name} ({Quantity})";
    }

    public ItemSlot CreateInstance()
    {
        ItemSlot instance = Duplicate() as ItemSlot;
		
        instance.Item = Item?.CreateInstance();
        instance.Quantity = Quantity;

        return instance;
    }

}
