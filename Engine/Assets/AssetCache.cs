using MyEngine.Diagnostics;
using MyEngine.Rendering;
using Silk.NET.OpenGL;

namespace MyEngine.Assets;

/// <summary>
/// Кэш ассетов с подсчётом ссылок.
///
/// Использование:
///   var tex = cache.GetTexture("Assets/Textures/player.png");  // берём ссылку
///   // ... используем ...
///   cache.ReleaseTexture(tex);  // отдаём обратно
///
/// Если один путь запрашивают дважды — оба получают один объект.
/// Текстура освобождается, когда RefCount = 0.
///
/// ВАЖНО: каждый Get должен сопровождаться Release. Иначе утечка.
/// Пока никто не реализует это строго — но для типичной игры с долгоживущими
/// объектами это не проблема: загрузили один раз, живёт всю игру.
/// </summary>
public sealed class AssetCache : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, AssetRefCount<Texture2D>> _textures = new();
    private readonly Dictionary<string, AssetRefCount<Font>> _fonts = new();
    private readonly Dictionary<string, string> _pathAliases = new();

    public AssetCache(GL gl) => _gl = gl;

    /// <summary>Сколько сейчас загружено текстур.</summary>
    public int TextureCount => _textures.Count;

    /// <summary>Сколько сейчас загружено шрифтов.</summary>
    public int FontCount => _fonts.Count;

    /// <summary>Общее количество ссылок на все текстуры.</summary>
    public int TotalTextureRefs
    {
        get
        {
            int n = 0;
            foreach (var t in _textures.Values) n += t.RefCount;
            return n;
        }
    }

    /// <summary>
    /// Зарегистрировать "псевдоним" для пути.
    /// Например, "player" → "Assets/Textures/player.png".
    /// Тогда можно запрашивать коротко.
    /// </summary>
    public void RegisterAlias(string alias, string path)
    {
        _pathAliases[alias] = path;
    }

    private string ResolvePath(string path)
        => _pathAliases.TryGetValue(path, out var real) ? real : path;

    // ============================================================
    // Текстуры
    // ============================================================

    /// <summary>Получить текстуру. Увеличивает RefCount.</summary>
    public Texture2D GetTexture(string path)
    {
        path = ResolvePath(path);

        if (_textures.TryGetValue(path, out var entry))
        {
            entry.RefCount++;
            return entry.Asset;
        }

        var tex = new Texture2D(_gl, path);
        _textures[path] = new AssetRefCount<Texture2D>(tex);
        Log.Debug("AssetCache", $"Loaded texture: {path}");

        return tex;
    }

    /// <summary>Отдать текстуру. Уменьшает RefCount. При 0 — Dispose.</summary>
    public void ReleaseTexture(Texture2D texture)
    {
        foreach (var kv in _textures)
        {
            if (kv.Value.Asset != texture) continue;

            kv.Value.RefCount--;
            if (kv.Value.RefCount <= 0)
            {
                texture.Dispose();
                _textures.Remove(kv.Key);
                Log.Debug("AssetCache", $"Unloaded texture: {kv.Key}");
            }
            return;
        }
    }

    // ============================================================
    // Шрифты
    // ============================================================

    /// <summary>Получить шрифт. Одинаковый (path, size) → один объект.</summary>
    public Font GetFont(string path, float size = 20f)
    {
        path = ResolvePath(path);
        var key = $"{path}@{size}";

        if (_fonts.TryGetValue(key, out var entry))
        {
            entry.RefCount++;
            return entry.Asset;
        }

        var font = new Font(_gl, path, size);
        _fonts[key] = new AssetRefCount<Font>(font);
        Log.Debug("AssetCache", $"Loaded font: {key}");

        return font;
    }

    public void ReleaseFont(Font font)
    {
        foreach (var kv in _fonts)
        {
            if (kv.Value.Asset != font) continue;

            kv.Value.RefCount--;
            if (kv.Value.RefCount <= 0)
            {
                font.Dispose();
                _fonts.Remove(kv.Key);
                Log.Debug("AssetCache", $"Unloaded font: {kv.Key}");
            }
            return;
        }
    }

    // ============================================================
    // Очистка
    // ============================================================

    /// <summary>
    /// Принудительно выгрузить все ресурсы, независимо от RefCount.
    /// Вызывать при закрытии игры.
    /// </summary>
    public void Clear()
    {
        foreach (var t in _textures.Values) t.Asset.Dispose();
        foreach (var f in _fonts.Values) f.Asset.Dispose();
        _textures.Clear();
        _fonts.Clear();
        _pathAliases.Clear();
    }

    public void Dispose() => Clear();
}