namespace SneakGame;

using Godot;

public partial class NpcActionRun : NonPlayerAction
{

    public override bool Execute(NonPlayerBrain owner)
    {
        bool success = base.Execute(owner);
        if (!success) return false;

        return true;
    }

    public override bool CanExecute(NonPlayerBrain owner)
    {
        return base.CanExecute(owner);
    }

}
