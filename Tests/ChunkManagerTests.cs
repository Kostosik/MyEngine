using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.WorldEngine;
using System.Numerics;

namespace Tests;

public class ChunkManagerTests
{
    // chunkSize=32 тайлов, tileSize=16 → chunk = 512 пикселей
    private static ChunkManager MakeManager() => new(chunkSize: 32, tileSize: 16f);

    // ============================================================
    // Координаты
    // ============================================================

    [Fact]
    public void WorldToChunk_Correctly()
    {
        var m = MakeManager();   // chunk world size = 512

        Assert.Equal(new ChunkCoord(0, 0), m.WorldToChunk(new Vector2(100, 200)));
        Assert.Equal(new ChunkCoord(1, 0), m.WorldToChunk(new Vector2(600, 100)));
        Assert.Equal(new ChunkCoord(0, 1), m.WorldToChunk(new Vector2(100, 700)));
        Assert.Equal(new ChunkCoord(-1, -1), m.WorldToChunk(new Vector2(-1, -1)));
    }

    [Fact]
    public void ChunkToWorld_Correctly()
    {
        var m = MakeManager();
        Assert.Equal(new Vector2(0, 0), m.ChunkToWorld(new ChunkCoord(0, 0)));
        Assert.Equal(new Vector2(512, 512), m.ChunkToWorld(new ChunkCoord(1, 1)));
        Assert.Equal(new Vector2(-512, 0), m.ChunkToWorld(new ChunkCoord(-1, 0)));
    }

    [Fact]
    public void ChunkCenter_Correctly()
    {
        var m = MakeManager();
        var center = m.ChunkCenter(new ChunkCoord(0, 0));
        Assert.Equal(256f, center.X);
        Assert.Equal(256f, center.Y);
    }

    // ============================================================
    // Создание и поиск
    // ============================================================

    [Fact]
    public void GetOrCreate_CreatesOnce()
    {
        var m = MakeManager();

        var c1 = m.GetOrCreateChunk(new Vector2(100, 100));
        var c2 = m.GetOrCreateChunk(new Vector2(200, 200));

        Assert.Same(c1, c2);   // оба в (0,0) чанке
        Assert.Equal(1, m.TotalCount);
    }

    [Fact]
    public void GetChunk_Missing_ReturnsNull()
    {
        var m = MakeManager();
        Assert.Null(m.GetChunk(new Vector2(100, 100)));
    }

    // ============================================================
    // Загрузка / выгрузка
    // ============================================================

    [Fact]
    public void LoadAround_CreatesChunksInRadius()
    {
        var m = MakeManager();
        int created = m.LoadAround(Vector2.Zero, radiusChunks: 1);

        // Радиус 1 → квадрат 3×3 = 9 чанков
        Assert.Equal(9, created);
        Assert.Equal(9, m.LoadedCount);
    }

    [Fact]
    public void LoadAround_SecondCall_DoesNotDuplicate()
    {
        var m = MakeManager();
        m.LoadAround(Vector2.Zero, 1);
        int created = m.LoadAround(Vector2.Zero, 1);

        Assert.Equal(0, created);
        Assert.Equal(9, m.LoadedCount);
    }

    [Fact]
    public void UnloadFar_RemovesDistant()
    {
        var m = MakeManager();
        m.LoadAround(Vector2.Zero, radiusChunks: 3);

        int unloaded = m.UnloadFar(Vector2.Zero, radiusChunks: 1);

        Assert.True(unloaded > 0, "Что-то должно выгрузиться");
        Assert.True(m.LoadedCount <= 9, "Остались только ближние");
    }

    [Fact]
    public void UnloadFar_KeepsNearbyLoaded()
    {
        var m = MakeManager();
        m.LoadAround(Vector2.Zero, 2);
        m.UnloadFar(Vector2.Zero, 2);

        // Всё в радиусе 2 осталось загруженным
        Assert.Equal(25, m.LoadedCount); // 5×5
    }

    // ============================================================
    // Индекс сущностей
    // ============================================================

    [Fact]
    public void RebuildEntityIndex_DistributesEntities()
    {
        var m = MakeManager();
        var world = new World();

        // Сущности в разных чанках
        for (int i = 0; i < 10; i++)
        {
            var e = world.Create();
            e.Add(new Transform { Position = new Vector2(100 + i * 20, 100) });
        }
        // Ещё 5 — в другом чанке
        for (int i = 0; i < 5; i++)
        {
            var e = world.Create();
            e.Add(new Transform { Position = new Vector2(600 + i * 20, 600) });
        }

        m.RebuildEntityIndex(world);

        // Должно быть как минимум 2 чанка
        Assert.True(m.TotalCount >= 2);
    }

    [Fact]
    public void QueryEntities_FindsNearby()
    {
        var m = MakeManager();
        var world = new World();

        // Сущности в радиусе 100 от нуля
        for (int i = 0; i < 5; i++)
        {
            var e = world.Create();
            e.Add(new Transform { Position = new Vector2(i * 10, 0) });
        }
        // Далеко
        for (int i = 0; i < 5; i++)
        {
            var e = world.Create();
            e.Add(new Transform { Position = new Vector2(2000 + i * 10, 0) });
        }

        m.RebuildEntityIndex(world);

        var results = new List<Entity>();
        m.QueryEntities(Vector2.Zero, radius: 100f, results);

        Assert.Equal(5, results.Count);
    }

    // ============================================================
    // Chunk тайлов
    // ============================================================

    [Fact]
    public void Chunk_WorldToTile_Correctly()
    {
        var m = MakeManager();
        var chunk = m.GetOrCreateChunk(Vector2.Zero);

        // tileSize = 16 → тайл (1, 2) в мире = (16, 32)
        var (tx, ty) = chunk.WorldToTile(new Vector2(20, 40), 16f);
        Assert.Equal(1, tx);
        Assert.Equal(2, ty);
    }

    [Fact]
    public void Chunk_WorldToTile_Outside_ReturnsNegative()
    {
        var m = MakeManager();
        var chunk = m.GetOrCreateChunk(Vector2.Zero);

        var (tx, ty) = chunk.WorldToTile(new Vector2(-10, -10), 16f);
        Assert.Equal(-1, tx);
        Assert.Equal(-1, ty);
    }

    [Fact]
    public void Chunk_TileToWorld_Correctly()
    {
        var m = MakeManager();
        var chunk = m.GetOrCreateChunk(Vector2.Zero);

        var world = chunk.TileToWorld(2, 3, tileSize: 16f);
        Assert.Equal(32f, world.X);
        Assert.Equal(48f, world.Y);
    }
}