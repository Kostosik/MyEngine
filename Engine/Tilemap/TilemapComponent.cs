using MyEngine.WorldEngine;
using System.Numerics;

namespace MyEngine.Tilemap;

/// <summary>
/// Тайлмап. Хранит метаданные (Tileset, слои, размеры) и тайлы,
/// разложенные по чанкам.
///
/// Доступ к тайлам — через GetTile/SetTile по **мировым тайловым координатам**.
/// Внутри делегирует в нужный чанк.
/// </summary>
public sealed class TilemapComponent
{
    public Tileset Tileset = null!;
    public List<TilemapLayer> Layers = new();

    /// <summary>Размер всей карты в тайлах.</summary>
    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>Размер одного чанка в тайлах.</summary>
    public int ChunkSize { get; private set; } = 32;

    /// <summary>Мировая позиция левого верхнего угла карты.</summary>
    public Vector2 Origin;

    /// <summary>Чанки тайлов. Ключ — координата чанка.</summary>
    private readonly Dictionary<ChunkCoord, TilemapChunk> _chunks = new();

    public int TileSize => Tileset.TileSize;
    public int ChunkCount => _chunks.Count;

    // ============================================================
    // Инициализация
    // ============================================================

    public void Initialize(int width, int height, int chunkSize = 32)
    {
        if (chunkSize <= 0) throw new ArgumentException("chunkSize must be > 0");
        Width = width;
        Height = height;
        ChunkSize = chunkSize;
    }

    public void AddLayer(TilemapLayer layer)
    {
        layer.Index = Layers.Count;
        Layers.Add(layer);
    }

    public void SortLayers()
    {
        Layers.Sort((a, b) => a.DrawOrder.CompareTo(b.DrawOrder));
        for (int i = 0; i < Layers.Count; i++)
            Layers[i].Index = i;
    }

    public TilemapLayer? GetLayer(string name)
        => Layers.Find(l => l.Name == name);

    // ============================================================
    // Доступ к тайлам
    // ============================================================

    public int GetTile(int layerIndex, int tileX, int tileY)
    {
        var coord = TileToChunk(tileX, tileY);
        if (!_chunks.TryGetValue(coord, out var chunk)) return -1;

        var (lx, ly) = TileToLocal(tileX, tileY, coord);
        return chunk.Get(layerIndex, lx, ly);
    }

    public void SetTile(int layerIndex, int tileX, int tileY, int tile)
    {
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height) return;

        var coord = TileToChunk(tileX, tileY);
        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            chunk = new TilemapChunk(ChunkSize, Layers.Count);
            _chunks[coord] = chunk;
        }

        var (lx, ly) = TileToLocal(tileX, tileY, coord);
        chunk.Set(layerIndex, lx, ly, tile);
    }

    // ============================================================
    // Итерация по чанкам
    // ============================================================

    public IEnumerable<(ChunkCoord coord, TilemapChunk chunk)> AllChunks()
    {
        foreach (var kv in _chunks)
            yield return (kv.Key, kv.Value);
    }

    public TilemapChunk? GetChunk(ChunkCoord coord)
        => _chunks.GetValueOrDefault(coord);

    public void ClearChunks() => _chunks.Clear();

    // ============================================================
    // Координаты
    // ============================================================

    public ChunkCoord TileToChunk(int tileX, int tileY)
        => new(tileX / ChunkSize, tileY / ChunkSize);

    public (int lx, int ly) TileToLocal(int tileX, int tileY, ChunkCoord coord)
    {
        int lx = tileX - coord.X * ChunkSize;
        int ly = tileY - coord.Y * ChunkSize;
        return (lx, ly);
    }

    public (int x, int y) WorldToTile(Vector2 world)
    {
        int x = (int)System.MathF.Floor((world.X - Origin.X) / TileSize);
        int y = (int)System.MathF.Floor((world.Y - Origin.Y) / TileSize);
        return (x, y);
    }

    public Vector2 TileToWorld(int x, int y)
        => Origin + new Vector2(x * TileSize, y * TileSize);

    public Vector2 TileCenter(int x, int y)
        => TileToWorld(x, y) + new Vector2(TileSize * 0.5f, TileSize * 0.5f);
}