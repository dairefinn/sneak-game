namespace SneakGame;

using System.Linq;
using Godot;

public partial class SpawningHandler : Node
{

    [Export] public bool Enabled { get; set; } = true;
    [Export] public PackedScene PlayerScene { get; set; }

    public override void _Ready()
    {
        SpawnPlayer();
    }

    private void Initialize()
    {
        if (!Enabled) return;
        GD.Print("Initializing SpawningHandler");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
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
        var player = PlayerScene.Instantiate<Player>();
        player.Name = "Player";
        GetTree().CurrentScene.AddChild(player);
        player.GlobalPosition = position;
    }

}
