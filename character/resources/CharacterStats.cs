namespace SneakGame;

using Godot;

[GlobalClass]
public partial class CharacterStats : Resource
{

    [Signal] public delegate void HealthChangedEventHandler(int previousHealth);
    [Signal] public delegate void StaminaChangedEventHandler(int previousStamina);


    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int MaxStamina { get; set; } = 100;

    [Export] public int CurrentHealth {
        get => _currentHealth;
        set => SetCurrentHealth(value);
    }
    private int _currentHealth = 100;

    [Export] public int CurrentStamina {
        get => _currentStamina;
        set => SetCurrentStamina(value);
    }
    private int _currentStamina = 100;

    private void SetCurrentHealth(int value)
    {
        var previousHealth = _currentHealth;
        _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        EmitSignal(SignalName.HealthChanged, previousHealth);
    }

    private void SetCurrentStamina(int value)
    {
        var previousStamina = _currentStamina;
        _currentStamina = Mathf.Clamp(value, 0, MaxStamina);
        EmitSignal(SignalName.StaminaChanged, previousStamina);
    }

    public CharacterStats CreateInstance()
    {
        CharacterStats instance = Duplicate() as CharacterStats;
        
        instance.MaxHealth = MaxHealth;
        instance.MaxStamina = MaxStamina;

        instance.CurrentHealth = CurrentHealth;
        instance.CurrentStamina = CurrentStamina;

        return instance;
    }

}
