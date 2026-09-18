namespace MyEngine.Tilemap;

/// <summary>
/// Описание слоя тайлмапа. Сами тайлы хранятся в TilemapChunk.
/// </summary>
public sealed class TilemapLayer
{
    public string Name { get; }
    public bool Solid { get; set; }
    public int DrawOrder { get; set; }

    /// <summary>Индекс слоя в списке layers. Устанавливается при AddLayer.</summary>
    public int Index { get; internal set; }

    public TilemapLayer(string name)
    {
        Name = name;
    }
}