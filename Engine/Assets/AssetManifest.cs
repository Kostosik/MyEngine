using MyEngine.Diagnostics;
using System.Text.Json;

namespace MyEngine.Assets;

/// <summary>
/// Таблица «имя → абсолютный путь» для ассетов.
/// Загружается один раз при старте игры из Assets/Manifest.json.
///
/// Игра не строит пути вручную:
///   string path = Assets.Path("player");           // Assets/Textures/player_walk.png
///   string path = Assets.PathTexture("player");
///
/// Если имя не найдено — Warn в лог, возвращает fallback
/// (путь в корне по имени).
/// </summary>
public sealed class AssetManifest
{
    private readonly Dictionary<string, string> _textures = new();
    private readonly Dictionary<string, string> _fonts = new();
    private readonly Dictionary<string, string> _sounds = new();
    private readonly Dictionary<string, string> _maps = new();
    private readonly Dictionary<string, string> _data = new();

    /// <summary>Корневая папка ассетов (абсолютный путь).</summary>
    public string RootPath { get; private set; } = "";

    /// <summary>Загружен ли манифест. Если нет — все Path() возвращают fallback.</summary>
    public bool Loaded { get; private set; }

    // ============================================================
    // Загрузка
    // ============================================================

    /// <summary>
    /// Загрузить манифест. Если файла нет — Warn, но не крашим.
    /// Игра работает с fallback-путями.
    /// </summary>
    public void Load(string manifestPath)
    {
        RootPath = Path.GetDirectoryName(manifestPath) ?? "";

        if (!File.Exists(manifestPath))
        {
            Log.Warn("Assets", $"Manifest not found: {manifestPath}. Using fallback paths.");
            return;
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            var data = JsonSerializer.Deserialize<AssetManifestData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AssetManifestData();

            RootPath = Path.Combine(
                Path.GetDirectoryName(manifestPath) ?? "",
                data.Root);

            Fill(_textures, data.Textures);
            Fill(_fonts, data.Fonts);
            Fill(_sounds, data.Sounds);
            Fill(_maps, data.Maps);
            Fill(_data, data.Data);

            Loaded = true;
            Log.Info("Assets", $"Manifest loaded: {Count} entries");
        }
        catch (Exception ex)
        {
            Log.Error("Assets", $"Failed to load manifest: {ex.Message}");
        }
    }

    private static void Fill(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var kv in source)
            target[kv.Key] = kv.Value;
    }

    public int Count =>
        _textures.Count + _fonts.Count + _sounds.Count + _maps.Count + _data.Count;

    // ============================================================
    // Поиск путей
    // ============================================================

    public string PathTexture(string name) => Resolve(_textures, name, "Textures");
    public string PathFont(string name) => Resolve(_fonts, name, "Fonts");
    public string PathSound(string name) => Resolve(_sounds, name, "Sounds");
    public string PathMap(string name) => Resolve(_maps, name, "Maps");
    public string PathData(string name) => Resolve(_data, name, "Data");

    private string Resolve(Dictionary<string, string> table, string name, string fallbackFolder)
    {
        if (table.TryGetValue(name, out var relative))
            return Path.GetFullPath(Path.Combine(RootPath, relative));

        Log.Warn("Assets", $"Asset '{name}' not found in manifest. Fallback used.");
        return Path.GetFullPath(Path.Combine(RootPath, fallbackFolder, name));
    }
}