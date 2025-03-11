namespace SneakGame;

using System.Linq;
using Godot;

public partial class SpawningHandler : Node
{

    [Export] public bool Enabled { get; set; } = true;
    [Export] public PackedScene PlayerScene { get; set; }
    [Export] public CharacterStatsUI CharacterStatsUI { get; set; }
    [Export] public CameraController CameraController { get; set; }

    public override void _Ready()
    {
        if (!Enabled) return;
        GD.Print("Initializing SpawningHandler");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (!Enabled) return;

        SpawnPoint spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint != null)
        {
            CallDeferred(MethodName.CreatePlayerAt, spawnPoint.GlobalPosition);
        }
    }

    private SpawnPoint GetRandomSpawnPoint()
    {
        var spawnPoints = GetTree().GetNodesInGroup("spawn_points").OfType<SpawnPoint>().ToList();

        GD.Print("Found spawn points: " + spawnPoints.Count);

        if (spawnPoints.Count == 0)
        {
            GD.PrintErr("No spawn points found");
            return null;
        }

        return spawnPoints[0];
    }

    private void CreatePlayerAt(Vector3 position)
    {
        Player player = PlayerScene.Instantiate<Player>();
        player.Character = player.Character.CreateInstance();
        player.Name = "Player";
        GetTree().CurrentScene.AddChild(player);
        player.GlobalPosition = position;

        CharacterStatsUI?.SetCharacterStats(player.Character.Stats);
        GD.Print("Player spawned at " + position);

        Globals.GetInstance().CurrentPlayer = player;

        CameraController.FocusedEntity = player;
        player.MovementController.CameraController = CameraController;
    }

}
