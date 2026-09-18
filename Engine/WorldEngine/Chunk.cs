using MyEngine.Ecs;
using MyEngine.WorldEngine;

namespace MyEngine.WorldEngine;

/// <summary>
/// Один чанк мира — блок N×N тайлов.
///
/// Содержит:
///   - Список сущностей, чьи позиции попадают в этот регион.
///   - Заготовка под тайлы (для будущего Tilemap — просто массив id).
///   - Флаг загруженности (Loaded).
///
/// Чанк НЕ владеет сущностями — они остаются в World.
/// Чанк хранит только индексы для быстрого доступа по региону.
/// </summary>
public sealed class Chunk
{
    public ChunkCoord Coord { get; }
    public int Size { get; }

    /// <summary>Позиция левого верхнего угла чанка в мировых координатах.</summary>
    public System.Numerics.Vector2 WorldMin { get; }

    /// <summary>Загружен ли чанк. Не загруженные можно выгрузить/не обновлять.</summary>
    public bool Loaded { get; set; }

    /// <summary>Сущности, чьи позиции попадают в этот чанк.</summary>
    public List<Entity> Entities { get; } = new();

    /// <summary>
    /// Тайлы этого чанка. Пока null — tilemap ещё не добавлен.
    /// Когда добавим: массив int размера Size*Size с индексами тайлов.
    /// </summary>
    public int[]? Tiles;

    public Chunk(ChunkCoord coord, int size, System.Numerics.Vector2 worldMin)
    {
        Coord = coord;
        Size = size;
        WorldMin = worldMin;
        Loaded = true;
    }

    /// <summary>Мировые координаты для тайла (localX, localY) внутри чанка.</summary>
    public System.Numerics.Vector2 TileToWorld(int localX, int localY, float tileSize)
        => WorldMin + new System.Numerics.Vector2(localX * tileSize, localY * tileSize);

    /// <summary>Локальный тайл из мировых координат. (-1,-1), если вне чанка.</summary>
    public (int x, int y) WorldToTile(System.Numerics.Vector2 world, float tileSize)
    {
        int lx = (int)System.MathF.Floor((world.X - WorldMin.X) / tileSize);
        int ly = (int)System.MathF.Floor((world.Y - WorldMin.Y) / tileSize);
        if (lx < 0 || ly < 0 || lx >= Size || ly >= Size) return (-1, -1);
        return (lx, ly);
    }
}