using MyEngine.Rendering;

namespace MyEngine.Tilemap;

/// <summary>
/// Атлас тайлов. Текстура с сеткой тайлов одинакового размера.
/// Один Tileset используется многими TilemapLayer.
///
/// Например: текстура 256×256, tileSize = 32 → 8×8 = 64 тайла.
/// </summary>
public sealed class Tileset
{
    public Texture2D Texture { get; }
    public int TileSize { get; }

    public int Columns { get; }
    public int Rows { get; }
    public int TileCount => Columns * Rows;

    /// <summary>Индексы проходимых тайлов. Остальные — стены.</summary>
    public HashSet<int> SolidTiles { get; } = new();

    public Tileset(Texture2D texture, int tileSize)
    {
        if (tileSize <= 0) throw new ArgumentException("tileSize must be > 0");
        Texture = texture;
        TileSize = tileSize;
        Columns = texture.Width / tileSize;
        Rows = texture.Height / tileSize;
    }

    /// <summary>UV-регион тайла в текстуре. (-1,-1) если индекс вне диапазона.</summary>
    public (int x, int y) GetTilePixel(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= TileCount) return (-1, -1);
        int col = tileIndex % Columns;
        int row = tileIndex / Columns;
        return (col * TileSize, row * TileSize);
    }

    public bool IsSolid(int tileIndex) => SolidTiles.Contains(tileIndex);
}