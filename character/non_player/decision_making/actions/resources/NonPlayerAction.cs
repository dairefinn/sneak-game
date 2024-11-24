namespace SneakGame;

using Godot;

[GlobalClass]
public partial class NonPlayerAction : Resource
{

    [Export] public string ActionName { get; set; } = string.Empty;
    [Export] public string ActionDescription { get; set; } = string.Empty;
    [Export] public int ActionCost { get; set; } = 0;

    public virtual bool Execute(NonPlayerBrain owner)
    {
        if (!CanExecute(owner)) return false;

        GD.Print("Executing action: " + ActionName);
        return true;
    }

    public virtual bool CanExecute(NonPlayerBrain owner)
    {
        return true;
    }

}
