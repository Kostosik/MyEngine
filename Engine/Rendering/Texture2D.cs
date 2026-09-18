using Silk.NET.OpenGL;
using StbImageSharp;
using MyEngine.Rendering.RHI;

namespace MyEngine.Rendering;

public sealed class Texture2D : IDisposable, ITexture
{
    private readonly GL _gl;
    private bool _ownsHandle = true;

    public uint Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    // --- Конструктор из файла ---
    public Texture2D(GL gl, string path)
    {
        _gl = gl;
        using var stream = File.OpenRead(path);
        var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        Width = img.Width;
        Height = img.Height;
        Handle = Upload(img.Data, img.Width, img.Height);
    }

    // --- Конструктор из сырых пикселей ---
    public Texture2D(GL gl, byte[] rgba, int w, int h)
    {
        _gl = gl;
        Width = w;
        Height = h;
        Handle = Upload(rgba, w, h);
    }

    // --- NEW: приватный конструктор для FromExisting ---
    private Texture2D(GL gl, uint handle, int width, int height, bool ownsHandle)
    {
        _gl = gl;
        Handle = handle;
        Width = width;
        Height = height;
        _ownsHandle = ownsHandle;
    }

    // --- NEW: обернуть существующую GL-текстуру ---
    /// <summary>
    /// Обернуть уже существующую GL-текстуру (например, из RenderTarget)
    /// как Texture2D. Используется для постобработки.
    ///
    /// ВАЖНО: этот Texture2D НЕ владеет текстурой. Dispose не удалит её —
    /// она удаляется владельцем (RenderTarget).
    /// </summary>
    public static Texture2D FromExisting(GL gl, uint handle, int width, int height)
        => new(gl, handle, width, height, ownsHandle: false);

    /// <summary>Белая 1×1 текстура для цветных квадратов.</summary>
    public static Texture2D White(GL gl)
        => new(gl, new byte[] { 255, 255, 255, 255 }, 1, 1);

    private unsafe uint Upload(byte[] data, int w, int h)
    {
        uint tex = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, tex);
        fixed (byte* p = data)
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba,
                (uint)w, (uint)h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        return tex;
    }

    public void Bind(int unit = 0)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + unit);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Unbind(int slot = 0)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + slot);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        // Текстура, обёрнутая из RenderTarget, не принадлежит нам — не удаляем
        if (_ownsHandle)
            _gl.DeleteTexture(Handle);
    }
}