using MyEngine.Diagnostics;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Один проход постобработки. Применяет шейдер к входной текстуре
/// и рисует результат на весь экран.
///
/// Параметры эффекта (uniform'ы) задаются через setup-функцию,
/// которая вызывается ПОСЛЕ glUseProgram. Это критично — иначе
/// uniform'ы уходят в предыдущую активную программу.
/// </summary>
public sealed class PostProcessShader : IDisposable, IPostProcessPass
{
    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly Action<Shader>? _setup;

    public PostProcessShader(GL gl, string fragmentShaderSource, Action<Shader>? setup = null)
    {
        _gl = gl;
        _setup = setup;

        // Вершинный шейдер один для всех эффектов — просто full-screen quad
        const string vertexSrc = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aUV;
out vec2 vUV;
void main() {
    vUV = aUV;
    gl_Position = vec4(aPos, 0.0, 1.0);
}";

        _shader = new Shader(gl, vertexSrc, fragmentShaderSource);

        // Full-screen quad: 4 вершины, 2 треугольника
        float[] vertices = {
            // x, y, u, v
            -1f, -1f, 0f, 0f,
             1f, -1f, 1f, 0f,
             1f,  1f, 1f, 1f,
            -1f,  1f, 0f, 1f,
        };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

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

    public Shader Shader => _shader;

    public Texture2D Process(Texture2D input, PostProcessContext context)
    {
        var rt = context.Rent(context.Width, context.Height);
        rt.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        Apply(input, context.Width, context.Height);

        rt.Unbind(context.Width, context.Height);
        return rt.GetColorTextureAsTexture2D();
    }

    /// <summary>Нарисовать full-screen quad в текущий framebuffer. Без биндинга текстур.</summary>
    public unsafe void DrawFullscreenQuad(int screenWidth, int screenHeight)
    {
        _gl.Disable(EnableCap.ScissorTest);
        _gl.Disable(EnableCap.Blend);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.ColorMask(true, true, true, true);
        _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
    }

    /// <summary>Применить эффект к текстуре и нарисовать на текущий framebuffer.</summary>
    public unsafe void Apply(Texture2D input, int screenWidth, int screenHeight)
    {
        // Полный сброс состояния — ImGui или SpriteBatch могли оставить что угодно
        _gl.Disable(EnableCap.ScissorTest);
        _gl.Disable(EnableCap.Blend);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.ColorMask(true, true, true, true);
        _gl.StencilMask(0xFF);
        _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

        _shader.Use();
        _shader.SetInt("uTexture", 0);

        // ВАЖНО: setup вызывается ПОСЛЕ Use — uniform'ы идут в этот шейдер
        _setup?.Invoke(_shader);

        input.Bind(0);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
    }

    public void Dispose()
    {
        _shader.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}