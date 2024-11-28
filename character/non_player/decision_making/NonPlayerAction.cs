namespace SneakGame;

using Godot;

[GlobalClass]
public partial class NonPlayerAction : Node
{
    
    // TODO: Not sure I want to keep a master list of actions. Might be better to discover them at runtime.
    public enum Type
    {
        ATTACK,
        FOLLOW,
        PATROL,
        RUN,
        WALK,
        IDLE
    }

    [Signal] public delegate void TransitionRequestedEventHandler(NonPlayerAction from, Type to);

    [Export] public string ActionName { get; set; } = string.Empty;
    [Export] public string ActionDescription { get; set; } = string.Empty;
    [Export] public int ActionCost { get; set; } = 0;
    
    
    public virtual Type ActionType { get; set; }
    public NonPlayerBrain Brain { get; set; }


    public virtual bool CanExecute()
    {
        return true;
    }

    public virtual void Enter()
    {
        return;
    }

    public virtual void PostEnter()
    {
        return;
    }

    public virtual void Exit()
    {
        return;
    }

    public virtual void OnProcess(double delta)
    {
        return;
    }

    public virtual void OnInput(InputEvent @event)
    {
        return;
    }

    // NOTE: This is a bit of a hack because the signals weren't being emitted properly from the brain
    public virtual void ChangeToAction(NonPlayerAction.Type to)
    {
        EmitSignal(SignalName.TransitionRequested, this, (int)to);
    }

}
