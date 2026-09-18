using Silk.NET.OpenGL;

namespace MyEngine.Rendering;

/// <summary>
/// Контекст одного кадра постобработки. Даёт эффектам доступ к
/// временным render target'ам — эффект может арендовать их на время
/// работы и вернуть обратно.
///
/// Один контекст на кадр, создаётся в PostProcessStack.Process.
/// </summary>
public sealed class PostProcessContext
{
    private readonly GL _gl;
    private readonly Stack<RenderTarget> _pool = new();
    private readonly List<RenderTarget> _inUse = new();

    public int Width { get; private set; }
    public int Height { get; private set; }

    public PostProcessContext(GL gl) => _gl = gl;

    internal void Begin(int width, int height)
    {
        Width = width;
        Height = height;
        _inUse.Clear();
    }

    internal void End()
    {
        // Возвращаем все арендованные RT в пул
        foreach (var rt in _inUse)
            _pool.Push(rt);
        _inUse.Clear();
    }

    /// <summary>
    /// Арендовать временный render target. Обязательно вернуть через Release
    /// или дождаться End (автоматически вернёт).
    /// </summary>
    public RenderTarget Rent(int width, int height)
    {
        RenderTarget rt;
        if (_pool.Count > 0)
        {
            rt = _pool.Pop();
            rt.Resize(width, height);
        }
        else
        {
            rt = new RenderTarget(_gl, width, height);
        }
        _inUse.Add(rt);
        return rt;
    }

    /// <summary>Вернуть RT в пул раньше конца кадра.</summary>
    public void Release(RenderTarget rt)
    {
        if (_inUse.Remove(rt))
            _pool.Push(rt);
    }

    /// <summary>Уничтожить все RT (вызывается при Dispose стека).</summary>
    internal void DisposeAll()
    {
        foreach (var rt in _pool) rt.Dispose();
        foreach (var rt in _inUse) rt.Dispose();
        _pool.Clear();
        _inUse.Clear();
    }
}