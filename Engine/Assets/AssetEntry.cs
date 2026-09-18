namespace MyEngine.Assets;

public enum AssetKind
{
    Unknown,
    Folder,
    Texture,    // .png, .jpg, .bmp
    Json,       // .json
    Text,       // .txt, .md
    Font,       // .ttf, .otf
    Audio,      // .wav, .ogg, .mp3
}

/// <summary>
/// Один элемент в Assets: файл или папка.
/// Хранит путь, тип, детей (для папок).
/// </summary>
public sealed class AssetEntry
{
    public string Name = "";
    public string FullPath = "";
    public string RelativePath = "";   // относительно корня Assets
    public AssetKind Kind = AssetKind.Unknown;
    public bool IsDirectory;

    public List<AssetEntry> Children = new();

    public long SizeBytes;

    public static AssetKind DetectKind(string path, bool isDirectory)
    {
        if (isDirectory) return AssetKind.Folder;

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".png" or ".jpg" or ".jpeg" or ".bmp" => AssetKind.Texture,
            ".json" => AssetKind.Json,
            ".txt" or ".md" => AssetKind.Text,
            ".ttf" or ".otf" => AssetKind.Font,
            ".wav" or ".ogg" or ".mp3" => AssetKind.Audio,
            _ => AssetKind.Unknown
        };
    }
}