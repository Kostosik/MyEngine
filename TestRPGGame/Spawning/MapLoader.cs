using MyEngine.Ai;
using MyEngine.Ecs;
using System.Numerics;
using System.Text.Json;
using TestRPGGame.Data;

namespace TestRPGGame.Spawning;

public static class MapLoader
{
    // Границы карты — должны совпадать с твоим island.json
    private static readonly Vector2 MapMin = new(0, 0);
    private static readonly Vector2 MapMax = new(2000, 2000);
    private const float CellSize = 32f;

    public static NavGrid Load(World world, string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Map file not found: {path}");

        string json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<MapData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new MapData();

        // 1. Стены
        foreach (var w in data.Walls)
            EntityFactory.CreateWall(world, new Vector2(w.X, w.Y), new Vector2(w.W, w.H));

        // 2. Строим nav-сетку из этих стен
        var navGrid = NavGridBuilder.Build(world, CellSize, MapMin, MapMax);

        // 3. Враги — уже с navGrid
        foreach (var e in data.Enemies)
        {
            EntityFactory.CreateEnemy(
                world,
                new Vector2(e.X, e.Y),
                navGrid,
                hp: e.Hp,
                damage: e.Damage,
                moveSpeed: e.Speed,
                aggroRadius: e.AggroRadius,
                xpReward: e.Xp);
        }

        foreach (var n in data.Npcs)
            EntityFactory.CreateNpc(world, n.Id, n.Name, new Vector2(n.X, n.Y), n.Lines.ToArray());

        foreach (var p in data.Pickups)
            EntityFactory.CreatePickup(world, new Vector2(p.X, p.Y), p.Kind, p.Id);

        if (data.Lighthouse != null)
            EntityFactory.CreateLighthouse(world, new Vector2(data.Lighthouse.X, data.Lighthouse.Y));

        return navGrid;
    }
}