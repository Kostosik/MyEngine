using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering.RHI;

/// <summary>
/// OpenGL-реализация IRenderer. Все прямые вызовы GL идут через неё.
/// Это единственный класс, который знает про Silk.NET.OpenGL.
/// </summary>
public sealed class OpenGLRenderer : IRenderer
{
    private readonly GL _gl;
    private readonly uint _quadVao;
    private readonly uint _quadVbo;

    public GL GL => _gl; // публично, для обратной совместимости

    public OpenGLRenderer(GL gl)
    {
        _gl = gl;

        // Full-screen quad — используется в DrawFullscreenQuad
        float[] vertices = {
            -1f, -1f, 0f, 0f,
             1f, -1f, 1f, 0f,
             1f,  1f, 1f, 1f,
            -1f,  1f, 0f, 1f,
        };

        _quadVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_quadVao);

        _quadVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _quadVbo);
        unsafe
        {
            fixed (float* p = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(vertices.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);
        }

        unsafe
        {
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)0);
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                4 * sizeof(float), (void*)(2 * sizeof(float)));
        }
    }

    // ============================================================
    // Управление кадром
    // ============================================================

    public void Clear(Vector4 color)
    {
        _gl.ClearColor(color.X, color.Y, color.Z, color.W);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        _gl.Viewport(x, y, (uint)width, (uint)height);
    }

    public void SetRenderTarget(IRenderTarget? target, int screenWidth, int screenHeight)
    {
        if (target == null)
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);
        }
        else
        {
            target.Bind();
        }
    }

    // ============================================================
    // Состояние
    // ============================================================

    public void SetBlend(bool enabled)
    {
        if (enabled)
        {
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        else
        {
            _gl.Disable(EnableCap.Blend);
        }
    }

    public void SetDepthTest(bool enabled)
    {
        if (enabled) _gl.Enable(EnableCap.DepthTest);
        else _gl.Disable(EnableCap.DepthTest);
    }

    public void SetScissor(bool enabled, int x = 0, int y = 0, int w = 0, int h = 0)
    {
        if (enabled)
        {
            _gl.Enable(EnableCap.ScissorTest);
            _gl.Scissor(x, y, (uint)w, (uint)h);
        }
        else
        {
            _gl.Disable(EnableCap.ScissorTest);
        }
    }

    // ============================================================
    // Фабрики ресурсов
    // ============================================================

    public ITexture CreateTexture(byte[] rgbaPixels, int width, int height)
        => new Texture2D(_gl, rgbaPixels, width, height);

    public ITexture CreateTexture(string path)
        => new Texture2D(_gl, path);

    public ITexture CreateWhiteTexture()
        => Texture2D.White(_gl);

    public IShader CreateShader(string vertexSource, string fragmentSource)
        => new Shader(_gl, vertexSource, fragmentSource);

    public IShader CreateShaderFromFiles(string vertexPath, string fragmentPath)
    {
        string vs = File.ReadAllText(vertexPath);
        string fs = File.ReadAllText(fragmentPath);
        return CreateShader(vs, fs);
    }

    public IRenderTarget CreateRenderTarget(int width, int height)
        => new RenderTarget(_gl, width, height);

    // ============================================================
    // Отрисовка
    // ============================================================

    public void DrawFullscreenQuad()
    {
        _gl.BindVertexArray(_quadVao);
        _gl.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
    }

    public void DrawTriangles(int vertexCount)
    {
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertexCount);
    }

    public void DrawTrianglesInstanced(int vertexCount, int instanceCount)
    {
        _gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, (uint)vertexCount, (uint)instanceCount);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_quadVbo);
        _gl.DeleteVertexArray(_quadVao);
    }
}