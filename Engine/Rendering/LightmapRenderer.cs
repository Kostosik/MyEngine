using MyEngine.Components;
using MyEngine.Ecs;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Рендерит lightmap — текстуру, где хранится сумма света
/// (ambient + все point lights).
///
/// Затем LightingPass умножает сцену на эту текстуру.
///
/// Свет рисуется как радиальный градиент для каждого источника.
/// </summary>
public sealed class LightmapRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly Shader _lightShader;
    private readonly uint _vao;
    private readonly uint _vbo;

    public LightmapRenderer(GL gl)
    {
        _gl = gl;

        const string vs = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aLocal;

uniform vec2 uCenter;
uniform float uRadius;
uniform vec2 uResolution;

out vec2 vLocal;

void main() {
    vLocal = aLocal;

    vec2 worldPos = uCenter + aPos * uRadius;
    vec2 ndc = (worldPos / uResolution) * 2.0 - 1.0;
    ndc.y = -ndc.y;
    gl_Position = vec4(ndc, 0.0, 1.0);
}";

        const string fs = @"#version 330 core
in vec2 vLocal;         // -1..1 от центра света
uniform vec3 uColor;
uniform float uIntensity;
uniform float uFalloff;
out vec4 FragColor;
void main() {
    // Расстояние от центра (0 — центр, 1 — край)
    float d = length(vLocal);
    if (d > 1.0) discard;

    // Затухание
    float atten = pow(1.0 - d, uFalloff);
    FragColor = vec4(uColor * uIntensity * atten, 1.0);
}";

        _lightShader = new Shader(gl, vs, fs);

        // Quad -1..1 (6 вершин — 2 треугольника)
        float[] vertices = {
            // x, y, localX, localY
            -1f, -1f, -1f, -1f,
             1f, -1f,  1f, -1f,
             1f,  1f,  1f,  1f,
            -1f, -1f, -1f, -1f,
             1f,  1f,  1f,  1f,
            -1f,  1f, -1f,  1f,
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

    /// <summary>
    /// Нарисовать все источники света в текущий framebuffer.
    /// </summary>
    public void Render(World world, Camera2D camera, int screenWidth, int screenHeight)
    {
        // Ambient — заполняем светом
        var ambient = world.FirstWith<AmbientLight>()?.Get<AmbientLight>();
        var ambientColor = ambient?.Color ?? Vector3.One;

        _gl.ClearColor(ambientColor.X, ambientColor.Y, ambientColor.Z, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Blend: складываем свет (additive)
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        // Disable depth/scissor — свет это 2D
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.ScissorTest);

        _lightShader.Use();
        _lightShader.SetVector2("uResolution",
            new Vector2(screenWidth, screenHeight));

        foreach (var e in world.With<PointLight>())
        {
            var l = e.Get<PointLight>()!;
            var t = e.Get<Transform>();
            if (t == null) continue;

            // Пульсация радиуса
            float radius = l.Radius;
            if (l.Pulsate)
            {
                float pulse = MathF.Sin(l.Time * l.PulsateSpeed) * l.PulsateAmount;
                radius *= (1f + pulse);
            }

            // Мировая позиция → экранная (пиксели)
            var screenPos = camera.WorldToScreen(t.Position, screenWidth, screenHeight);

            _lightShader.SetVector2("uCenter", screenPos);
            _lightShader.SetFloat("uRadius", radius);
            _lightShader.SetVector3("uColor", l.Color);
            _lightShader.SetFloat("uIntensity", l.Intensity);
            _lightShader.SetFloat("uFalloff", l.Falloff);

            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        _gl.Disable(EnableCap.Blend);
        _gl.Enable(EnableCap.DepthTest);
    }

    public void Dispose()
    {
        _lightShader.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteVertexArray(_vao);
    }
}