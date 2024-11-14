namespace SneakGame;

using System.Linq;
using Godot;

public partial class SpawningHandler : Node
{

    [Export] public PackedScene PlayerScene { get; set; }

    public override void _Ready()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Node3D spawnPoint = GetRandomSpawnPoint();
        CallDeferred(MethodName.CreatePlayerAt, spawnPoint.GlobalPosition);
    }

    private Node3D GetRandomSpawnPoint()
    {
        var spawnPoints = GetTree().GetNodesInGroup("spawn_points").OfType<Node3D>().ToList();

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
