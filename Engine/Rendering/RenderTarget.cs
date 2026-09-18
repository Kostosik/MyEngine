using MyEngine.Rendering.RHI;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Цель для рендера: framebuffer + привязанная текстура + depth buffer.
///
/// Использование:
///   var rt = new RenderTarget(gl, width, height);
///   rt.Bind();
///   gl.Clear(...);    // рисуем в текстуру
///   rt.Unbind();
///   // теперь rt.ColorTexture — обычная текстура, можно рисовать её на экран
/// </summary>
public sealed class RenderTarget : IDisposable, IRenderTarget
{
    private readonly GL _gl;

    public uint Framebuffer { get; private set; }
    public uint ColorTextureHandle { get; private set; }
    public uint DepthBuffer { get; private set; }
    public ITexture ColorTexture => GetColorTextureAsTexture2D();
    public int Width { get; private set; }
    public int Height { get; private set; }

    public RenderTarget(GL gl, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;
        CreateResources();
    }

    private unsafe void CreateResources()
    {
        // 1. Framebuffer
        Framebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);

        // 2. Цветовая текстура
        ColorTextureHandle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, ColorTextureHandle);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
            (uint)Width, (uint)Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, ColorTextureHandle, 0);

        // 3. Depth + stencil buffer
        DepthBuffer = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
            InternalFormat.Depth24Stencil8, (uint)Width, (uint)Height);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer, DepthBuffer);

        // 4. Проверка
        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
            throw new Exception($"Framebuffer incomplete: {status}");

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>Сделать эту цель активной. Весь рендер пойдёт в неё.</summary>
    public void Bind()
    {

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        _gl.Disable(EnableCap.ScissorTest);   // ← NEW
    }

    /// <summary>Вернуться к рендеру на экран.</summary>
    public void Unbind(int screenWidth, int screenHeight)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);
    }

    /// <summary>
    /// Изменить размер. Полезно при resize окна.
    /// </summary>
    public void Resize(int width, int height)
    {
        if (width == Width && height == Height) return;
        Dispose();
        Width = width;
        Height = height;
        CreateResources();
    }

    /// <summary>
    /// Обернуть color texture как Texture2D для использования в SpriteBatch.
    /// </summary>
    public Texture2D GetColorTextureAsTexture2D()
        => Texture2D.FromExisting(_gl, ColorTextureHandle, Width, Height);


    public void Dispose()
    {
        _gl.DeleteFramebuffer(Framebuffer);
        _gl.DeleteTexture(ColorTextureHandle);
        _gl.DeleteRenderbuffer(DepthBuffer);
    }
}