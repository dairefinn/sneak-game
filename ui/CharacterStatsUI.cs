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
    private Label _healthLabel;
    private Label _staminaLabel;


    public override void _Ready()
    {
        base._Ready();

        _healthBar = GetNode<ColorRect>("%HealthBar");
        _staminaBar = GetNode<ColorRect>("%StaminaBar");
        _healthLabel = GetNode<Label>("%HealthLabel");
        _staminaLabel = GetNode<Label>("%StaminaLabel");

        GetTree().CreateTimer(2.0f, false).Timeout += () => {
            GD.Print("Setting Health and Stamina");
            CharacterStats.CurrentHealth = 50;
            CharacterStats.CurrentStamina = 50;
        };
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
        _healthBar.Size = new Vector2(BarWidth * healthPercentage, _healthBar.Size.Y);
        _healthLabel.Text = $"{_characterStats.CurrentHealth}/{_characterStats.MaxHealth}";
        GD.Print("Health changed to " + _characterStats.CurrentHealth);
    }

    private void OnStaminaChanged(int previousStamina)
    {
        if (!IsInstanceValid(_staminaBar)) return;
        float staminaPercentage = _characterStats.CurrentStamina / (float)_characterStats.MaxStamina;
        _staminaBar.Size = new Vector2(BarWidth * staminaPercentage, _staminaBar.Size.Y);
        _staminaLabel.Text = $"{_characterStats.CurrentStamina}/{_characterStats.MaxStamina}";
    }


}
