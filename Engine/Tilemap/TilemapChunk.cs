namespace MyEngine.Tilemap;

/// <summary>
/// Тайлы одного региона карты — для всех слоёв.
///
/// Layout: LayerTiles[layerIndex][localY * Size + localX]
/// -1 = пусто.
///
/// Не путать с World.Chunk (общий для любого мира).
/// Это отдельная структура, специфичная для tilemap.
/// </summary>
public sealed class TilemapChunk
{
    public int Size { get; }

    /// <summary>Тайлы по слоям. LayerTiles[layerIndex] — массив размера Size×Size.</summary>
    public int[][] LayerTiles { get; }

    public TilemapChunk(int size, int layerCount)
    {
        Size = size;
        LayerTiles = new int[layerCount][];
        for (int i = 0; i < layerCount; i++)
        {
            LayerTiles[i] = new int[size * size];
            System.Array.Fill(LayerTiles[i], -1);
        }
    }

    public bool InBounds(int lx, int ly)
        => lx >= 0 && ly >= 0 && lx < Size && ly < Size;

    public int Get(int layer, int lx, int ly)
    {
        if (layer < 0 || layer >= LayerTiles.Length) return -1;
        if (!InBounds(lx, ly)) return -1;
        return LayerTiles[layer][ly * Size + lx];
    }

    public void Set(int layer, int lx, int ly, int tile)
    {
        if (layer < 0 || layer >= LayerTiles.Length) return;
        if (!InBounds(lx, ly)) return;
        LayerTiles[layer][ly * Size + lx] = tile;
    }

    /// <summary>Есть ли хоть один непустой тайл на слое.</summary>
    public bool HasAnyTile(int layer)
    {
        if (layer < 0 || layer >= LayerTiles.Length) return false;
        foreach (var t in LayerTiles[layer])
            if (t >= 0) return true;
        return false;
    }
}