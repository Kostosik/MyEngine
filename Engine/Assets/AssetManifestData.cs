namespace MyEngine.Assets;

/// <summary>
/// Формат JSON манифеста ассетов.
/// </summary>
public sealed class AssetManifestData
{
    /// <summary>Корневая папка относительно exe. По умолчанию — "Assets".</summary>
    public string Root { get; set; } = "Assets";

    /// <summary>Имя → путь относительно Root.</summary>
    public Dictionary<string, string> Textures { get; set; } = new();
    public Dictionary<string, string> Fonts { get; set; } = new();
    public Dictionary<string, string> Sounds { get; set; } = new();
    public Dictionary<string, string> Maps { get; set; } = new();
    public Dictionary<string, string> Data { get; set; } = new();
}