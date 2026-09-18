using MyEngine.Diagnostics;
using MyEngine.Rendering;
using MyEngine.Threading;
using Silk.NET.OpenGL;
using StbImageSharp;
using System.Collections.Concurrent;

namespace MyEngine.Assets;

public sealed class ResourceManager : IDisposable
{
    private readonly GL _gl;
    private readonly JobSystem _jobs;

    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly HashSet<string> _loading = new();
    private readonly ConcurrentQueue<FinishedTexture> _finished = new();
    private readonly Texture2D _placeholder;

    private struct FinishedTexture
    {
        public string Path;
        public byte[] Data;
        public int Width;
        public int Height;
    }

    public ResourceManager(GL gl, JobSystem jobs)
    {
        _gl = gl;
        _jobs = jobs;

        // Заглушка — пурпурный квадрат 1×1, показывается пока текстура грузится
        _placeholder = new Texture2D(gl, new byte[] { 255, 0, 255, 255 }, 1, 1);
    }

    public Texture2D Placeholder => _placeholder;

    /// <summary>
    /// Возвращает текстуру сразу (если готова), либо заглушку.
    /// Параллельно запускает асинхронную загрузку в фоне.
    /// </summary>
    public Texture2D GetTexture(string path)
    {
        if (_textures.TryGetValue(path, out var tex))
            return tex;

        if (!_loading.Contains(path))
        {
            _loading.Add(path);
            StartAsyncLoad(path);
        }

        return _placeholder;
    }

    /// <summary>
    /// Вызывать каждый кадр на главном потоке.
    /// Подхватывает готовые текстуры и загружает их в GL.
    /// </summary>
    public void ProcessUploads()
    {
        while (_finished.TryDequeue(out var f))
        {
            try
            {
                var tex = new Texture2D(_gl, f.Data, f.Width, f.Height);
                _textures[f.Path] = tex;
                _loading.Remove(f.Path);
               Log.Info("[ResourceManager]", $" Loaded: {f.Path} ({f.Width}x{f.Height})");
            }
            catch (Exception ex)
            {
                Log.Error("[ResourceManager]", $" Failed to upload {f.Path}: {ex.Message}");
                _loading.Remove(f.Path);
            }
        }
    }

    private void StartAsyncLoad(string path)
    {
        // Запускаем через наш JobSystem — на одном из воркеров
        // (в реальном движке для такого используют отдельную "io" очередь,
        // но для обучения хватит и общей)
        _jobs.ParallelFor(0, 1, _ =>
        {
            try
            {
                using var stream = File.OpenRead(path);
                var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                _finished.Enqueue(new FinishedTexture
                {
                    Path = path,
                    Data = img.Data,
                    Width = img.Width,
                    Height = img.Height
                });
            }
            catch (Exception ex)
            {
                Log.Error("[ResourceManager]",$" Failed to decode {path}: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        foreach (var tex in _textures.Values) tex.Dispose();
        _placeholder.Dispose();
    }
}