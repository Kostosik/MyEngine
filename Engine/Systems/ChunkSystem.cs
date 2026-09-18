using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.WorldEngine;

namespace MyEngine.Systems;

/// <summary>
/// Подгружает и выгружает чанки вокруг игрока.
/// Перестраивает индекс сущностей по чанкам раз в N кадров.
///
/// Работает только если в мире есть ChunkManager (singleton).
/// </summary>
public sealed class ChunkSystem : ISystem
{
    private readonly ChunkManager _chunks;
    private readonly Entity _target;   // обычно игрок

    /// <summary>Радиус подгрузки в чанках.</summary>
    public int LoadRadius { get; set; } = 3;

    /// <summary>Радиус выгрузки в чанках. Должен быть больше LoadRadius.</summary>
    public int UnloadRadius { get; set; } = 5;

    /// <summary>Как часто перестраивать индекс сущностей (в кадрах).</summary>
    public int RebuildInterval { get; set; } = 30;

    private int _frameCounter;

    public SystemPhase Phase => SystemPhase.PostUpdate;
    public int Priority => 100;

    public ChunkSystem(ChunkManager chunks, Entity target)
    {
        _chunks = chunks;
        _target = target;
    }

    public void Update(World world, float dt)
    {
        var t = _target.Get<Transform>();
        if (t == null) return;

        _chunks.LoadAround(t.Position, LoadRadius);
        _chunks.UnloadFar(t.Position, UnloadRadius);

        _frameCounter++;
        if (_frameCounter >= RebuildInterval)
        {
            _frameCounter = 0;

            // Обновить ChunkRef для всех сущностей
            foreach (var e in world.With<ChunkRef>())
            {
                var et = e.Get<Transform>();
                var cr = e.Get<ChunkRef>()!;
                if (et == null) continue;

                var coord = _chunks.WorldToChunk(et.Position);
                cr.ChunkX = coord.X;
                cr.ChunkY = coord.Y;

                var chunk = _chunks.GetChunk(coord);
                cr.Loaded = chunk != null && chunk.Loaded;
            }
        }
    }
}