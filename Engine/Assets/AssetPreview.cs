using MyEngine.Rendering;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Assets;

/// <summary>
/// Кэш превью ассетов. Загружает по требованию, хранит,
/// освобождает по Dispose. Не грузит бинарные форматы целиком —
/// только то, что можно показать.
/// </summary>
public sealed class AssetPreview : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, string> _texts = new();
    private readonly int _maxTextSize = 16384;

    public AssetPreview(GL gl) => _gl = gl;

    public Texture2D? GetTexture(string path)
    {
        if (_textures.TryGetValue(path, out var t)) return t;

        if (!File.Exists(path)) return null;

        try
        {
            var tex = new Texture2D(_gl, path);
            _textures[path] = tex;
            return tex;
        }
        catch
        {
            return null;
        }
    }

    public string? GetText(string path)
    {
        if (_texts.TryGetValue(path, out var t)) return t;

        if (!File.Exists(path)) return null;

        try
        {
            var info = new FileInfo(path);
            if (info.Length > _maxTextSize)
                return $"(файл слишком большой: {info.Length} байт)";

            var text = File.ReadAllText(path);
            _texts[path] = text;
            return text;
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        foreach (var t in _textures.Values) t.Dispose();
        _textures.Clear();
        _texts.Clear();
    }

    public void Dispose() => Clear();
}