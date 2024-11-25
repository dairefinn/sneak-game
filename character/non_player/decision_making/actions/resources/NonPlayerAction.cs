namespace SneakGame;

using Godot;

[GlobalClass]
public partial class NonPlayerAction : Resource
{

    [Export] public string ActionName { get; set; } = string.Empty;
    [Export] public string ActionDescription { get; set; } = string.Empty;
    [Export] public int ActionCost { get; set; } = 0;

    public virtual bool Execute(NonPlayerBrain owner, double delta)
    {
        if (!CanExecute(owner)) return false;

        return true;
    }

    public virtual bool Terminate(NonPlayerBrain owner)
    {
        return true;
    }

    public virtual bool CanExecute(NonPlayerBrain owner)
    {
        return true;
    }

}
