namespace MyEngine.Rendering.RHI.Null;

/// <summary>
/// Render target-заглушка. Ничего не делает.
/// ColorTexture — NullTexture, чтобы код, читающий её, не падал.
/// </summary>
internal sealed class NullRenderTarget : IRenderTarget
{
    private readonly NullTexture _texture;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public ITexture ColorTexture => _texture;

    public NullRenderTarget(int width, int height)
    {
        Width = width;
        Height = height;
        _texture = new NullTexture(width, height);
    }

    public void Bind() { }
    public void Unbind(int screenWidth, int screenHeight) { }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Dispose() { }
}