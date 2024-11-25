namespace SneakGame;

using Godot;

public partial class CharacterStatsUI : Control
{

    [Export] public CharacterStats CharacterStats {
        get => _characterStats;
        set => SetCharacterStats(value);
    }
    private CharacterStats _characterStats;

    [Export] public int BarWidth { get; set; } = 80;


    private ColorRect _healthBar;
    private ColorRect _staminaBar;


    public override void _Ready()
    {
        base._Ready();

        _healthBar = GetNode<ColorRect>("%HealthBar");
        _staminaBar = GetNode<ColorRect>("%StaminaBar");

        // GetTree().CreateTimer(2.0f, false).Timeout += () => {
        //     CharacterStats.CurrentHealth = 50;
        //     CharacterStats.CurrentStamina = 50;
        //     GD.Print("Health and Stamina set");
        // };
    }


    public void SetCharacterStats(CharacterStats value)
    {
        _characterStats = value;

        value.HealthChanged += OnHealthChanged;
        value.StaminaChanged += OnStaminaChanged;
    }

    private void OnHealthChanged(int previousHealth)
    {
        if (!IsInstanceValid(_healthBar)) return;
        float healthPercentage = _characterStats.CurrentHealth / (float)_characterStats.MaxHealth;
        _healthBar.CustomMinimumSize = new Vector2(BarWidth * healthPercentage, _healthBar.CustomMinimumSize.Y);
        GD.Print("Health changed to " + _characterStats.CurrentHealth);
    }

    private void OnStaminaChanged(int previousStamina)
    {
        if (!IsInstanceValid(_staminaBar)) return;
        float staminaPercentage = _characterStats.CurrentStamina / (float)_characterStats.MaxStamina;
        _staminaBar.CustomMinimumSize = new Vector2(BarWidth * staminaPercentage, _staminaBar.CustomMinimumSize.Y);
    }


}
