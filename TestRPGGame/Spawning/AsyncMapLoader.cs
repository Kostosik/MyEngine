using MyEngine.Ai;
using MyEngine.Assets;
using MyEngine.Ecs;
using MyEngine.Threading;
using System.Numerics;
using System.Text.Json;
using TestRPGGame.Data;

namespace TestRPGGame.Spawning;

public sealed class AsyncMapLoader
{
    private readonly AsyncAssetLoader<MapData> _loader;

    public bool IsDone => _loader.IsDone;
    public Exception? Error => _loader.Error;

    // Границы карты — совпадают с island.json
    private static readonly Vector2 MapMin = new(0, 0);
    private static readonly Vector2 MapMax = new(2000, 2000);
    private const float CellSize = 32f;

    public AsyncMapLoader(JobSystem jobs)
    {
        _loader = new AsyncAssetLoader<MapData>(jobs);
    }

    public void Start(string path)
    {
        _loader.Start(() =>
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Map file not found: {path}");

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MapData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new MapData();
        });
    }

    /// <summary>
    /// Вызвать на главном потоке, когда IsDone.
    /// Возвращает построенный NavGrid — пригодится для дебага.
    /// </summary>
    public NavGrid ApplyTo(World world)
    {
        if (!_loader.IsDone) throw new InvalidOperationException("Map is not loaded yet");
        if (_loader.Error != null) throw _loader.Error;
        var data = _loader.Result;
        if (data == null) throw new InvalidOperationException("Map data is null");

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

        // 4. Остальное
        foreach (var n in data.Npcs)
            EntityFactory.CreateNpc(world, n.Id, n.Name, new Vector2(n.X, n.Y), n.Lines.ToArray());

        foreach (var p in data.Pickups)
            EntityFactory.CreatePickup(world, new Vector2(p.X, p.Y), p.Kind, p.Id);

        if (data.Lighthouse != null)
            EntityFactory.CreateLighthouse(world, new Vector2(data.Lighthouse.X, data.Lighthouse.Y));

        return navGrid;
    }
}