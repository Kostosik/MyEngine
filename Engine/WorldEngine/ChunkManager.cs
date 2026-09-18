using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.WorldEngine;
using System.Numerics;

namespace MyEngine.WorldEngine;

/// <summary>
/// Менеджер чанков. Делит мир на блоки фиксированного размера.
///
/// Использование:
///   var cm = new ChunkManager(chunkSize: 32, tileSize: 16f);
///   cm.UpdateAround(playerPos, radiusChunks: 2);  // подгрузить вокруг игрока
///   cm.UnloadFar(playerPos, radiusChunks: 4);     // выгрузить далёкие
///   var chunk = cm.GetChunk(worldPos);            // чанк по мировой позиции
/// </summary>
public sealed class ChunkManager
{
    private readonly Dictionary<ChunkCoord, Chunk> _chunks = new();

    /// <summary>Размер чанка в тайлах.</summary>
    public int ChunkSize { get; }

    /// <summary>Размер тайла в мировых единицах (пикселях).</summary>
    public float TileSize { get; }

    /// <summary>Размер чанка в мировых единицах.</summary>
    public float ChunkWorldSize => ChunkSize * TileSize;

    public int LoadedCount
    {
        get
        {
            int n = 0;
            foreach (var c in _chunks.Values) if (c.Loaded) n++;
            return n;
        }
    }

    public int TotalCount => _chunks.Count;

    public ChunkManager(int chunkSize = 32, float tileSize = 16f)
    {
        ChunkSize = chunkSize;
        TileSize = tileSize;
    }

    // ============================================================
    // Поиск чанков
    // ============================================================

    /// <summary>Чанк по мировым координатам. Создаётся, если не существует.</summary>
    public Chunk GetOrCreateChunk(Vector2 world)
    {
        var coord = WorldToChunk(world);
        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            var worldMin = ChunkToWorld(coord);
            chunk = new Chunk(coord, ChunkSize, worldMin);
            _chunks[coord] = chunk;
        }
        return chunk;
    }

    /// <summary>Существующий чанк или null.</summary>
    public Chunk? GetChunk(Vector2 world)
        => _chunks.GetValueOrDefault(WorldToChunk(world));

    public Chunk? GetChunk(ChunkCoord coord)
        => _chunks.GetValueOrDefault(coord);

    /// <summary>Все загруженные чанки.</summary>
    public IEnumerable<Chunk> LoadedChunks()
    {
        foreach (var c in _chunks.Values)
            if (c.Loaded) yield return c;
    }

    // ============================================================
    // Загрузка / выгрузка
    // ============================================================

    /// <summary>
    /// Подгрузить все чанки в радиусе вокруг точки.
    /// Возвращает количество созданных чанков.
    /// </summary>
    public int LoadAround(Vector2 center, int radiusChunks)
    {
        var centerCoord = WorldToChunk(center);
        int created = 0;

        for (int dy = -radiusChunks; dy <= radiusChunks; dy++)
        {
            for (int dx = -radiusChunks; dx <= radiusChunks; dx++)
            {
                var coord = new ChunkCoord(centerCoord.X + dx, centerCoord.Y + dy);
                if (!_chunks.TryGetValue(coord, out var chunk))
                {
                    var worldMin = ChunkToWorld(coord);
                    chunk = new Chunk(coord, ChunkSize, worldMin);
                    _chunks[coord] = chunk;
                    created++;
                }
                chunk.Loaded = true;
            }
        }

        return created;
    }

    /// <summary>
    /// Выгрузить чанки дальше радиуса. Данные сохраняются в памяти,
    /// но Loaded = false. Выгруженные чанки не обновляются в UpdateAround.
    /// </summary>
    public int UnloadFar(Vector2 center, int radiusChunks)
    {
        var centerCoord = WorldToChunk(center);
        int unloaded = 0;

        foreach (var chunk in _chunks.Values)
        {
            if (!chunk.Loaded) continue;
            int dx = System.Math.Abs(chunk.Coord.X - centerCoord.X);
            int dy = System.Math.Abs(chunk.Coord.Y - centerCoord.Y);
            if (dx > radiusChunks || dy > radiusChunks)
            {
                chunk.Loaded = false;
                unloaded++;
            }
        }

        return unloaded;
    }

    /// <summary>
    /// Полностью удалить чанк из памяти (для сохранения или жёсткой выгрузки).
    /// </summary>
    public void RemoveChunk(ChunkCoord coord)
    {
        _chunks.Remove(coord);
    }

    // ============================================================
    // Группировка сущностей
    // ============================================================

    /// <summary>
    /// Перестроить распределение сущностей по чанкам. Проходит по всем
    /// сущностям с Transform, определяет их чанк, добавляет в списки.
    ///
    /// ВАЖНО: вызывается НЕ каждый кадр. Либо при загрузке уровня,
    /// либо периодически (раз в 30 кадров), либо при изменении мира.
    /// </summary>
    public void RebuildEntityIndex(MyEngine.Ecs.World world)
    {
        // Очистить списки сущностей во всех чанках
        foreach (var chunk in _chunks.Values)
            chunk.Entities.Clear();

        // Разложить по чанкам
        foreach (var e in world.With<Transform>())
        {
            var t = e.Get<Transform>()!;
            var chunk = GetOrCreateChunk(t.Position);
            chunk.Entities.Add(e);
        }
    }

    /// <summary>
    /// Сущности в радиусе от точки. Возвращает через буфер.
    /// </summary>
    public void QueryEntities(Vector2 center, float radius, List<Entity> results)
    {
        results.Clear();

        int radiusInChunks = (int)System.MathF.Ceiling(radius / ChunkWorldSize) + 1;
        var centerCoord = WorldToChunk(center);
        float radiusSq = radius * radius;

        for (int dy = -radiusInChunks; dy <= radiusInChunks; dy++)
        {
            for (int dx = -radiusInChunks; dx <= radiusInChunks; dx++)
            {
                var coord = new ChunkCoord(centerCoord.X + dx, centerCoord.Y + dy);
                if (!_chunks.TryGetValue(coord, out var chunk)) continue;

                foreach (var e in chunk.Entities)
                {
                    var t = e.Get<Transform>();
                    if (t == null) continue;
                    if (Vector2.DistanceSquared(t.Position, center) <= radiusSq)
                        results.Add(e);
                }
            }
        }
    }

    // ============================================================
    // Утилиты координат
    // ============================================================

    public ChunkCoord WorldToChunk(Vector2 world)
    {
        int x = (int)System.MathF.Floor(world.X / ChunkWorldSize);
        int y = (int)System.MathF.Floor(world.Y / ChunkWorldSize);
        return new ChunkCoord(x, y);
    }

    public Vector2 ChunkToWorld(ChunkCoord coord)
        => new(coord.X * ChunkWorldSize, coord.Y * ChunkWorldSize);

    /// <summary>Центр чанка в мировых координатах.</summary>
    public Vector2 ChunkCenter(ChunkCoord coord)
        => ChunkToWorld(coord) + new Vector2(ChunkWorldSize * 0.5f, ChunkWorldSize * 0.5f);

    /// <summary>Очистить всё. При рестарте игры.</summary>
    public void Clear() => _chunks.Clear();
}