using Silk.NET.OpenGL;

namespace MyEngine.Rendering;

/// <summary>
/// Стек эффектов постобработки.
///
/// Каждый эффект — IPostProcessPass. Он может делать один проход
/// (PostProcessShader) или несколько (BloomPass с 4 проходами).
///
/// Стек больше не управляет ping-pong напрямую — это делает
/// PostProcessContext, который выдаёт эффектам временные render target'ы
/// через Rent/Release.
/// </summary>
public sealed class PostProcessStack : IDisposable
{
    private readonly GL _gl;
    private readonly List<IPostProcessPass> _passes = new();
    private readonly PostProcessContext _context;
    private int _width, _height;

    public PostProcessStack(GL gl, int width, int height)
    {
        _gl = gl;
        _context = new PostProcessContext(gl);
        _width = width;
        _height = height;
    }

    public void Add(IPostProcessPass pass) => _passes.Add(pass);
    public void Remove(IPostProcessPass pass) => _passes.Remove(pass);
    public void Clear() => _passes.Clear();

    public int Count => _passes.Count;

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        // Временные RT в пуле пересоздадутся при следующем Rent.
    }

    /// <summary>
    /// Прогнать входную текстуру через все эффекты.
    /// Возвращает итоговую текстуру для вывода на экран.
    /// </summary>
    public Texture2D Process(Texture2D input, int screenWidth, int screenHeight)
    {
        if (_passes.Count == 0) return input;

        _context.Begin(screenWidth, screenHeight);
        try
        {
            var current = input;
            foreach (var pass in _passes)
                current = pass.Process(current, _context);
            return current;
        }
        finally
        {
            _context.End();
        }
    }

    public void Dispose()
    {
        _context.DisposeAll();

        foreach (var pass in _passes)
            if (pass is IDisposable d) d.Dispose();

        _passes.Clear();
    }


}