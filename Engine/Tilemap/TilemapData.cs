namespace MyEngine.Tilemap;

/// <summary>
/// Формат JSON для загрузки тайлмапа.
/// </summary>
public sealed class TilemapData
{
    public string Tileset { get; set; } = "";    // имя файла относительно JSON
    public int TileSize { get; set; } = 32;
    public int Width { get; set; }
    public int Height { get; set; }
    public List<TilemapLayerData> Layers { get; set; } = new();
}

public sealed class TilemapLayerData
{
    public string Name { get; set; } = "";
    public bool Solid { get; set; }
    public int DrawOrder { get; set; }
    public List<int> Tiles { get; set; } = new();
}