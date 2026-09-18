using MyEngine.Components;
using MyEngine.Ecs;
using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Instanced рендерер спрайтов. Рисует N спрайтов одним draw call.
///
/// Использует один quad + instance buffer с данными на каждый спрайт.
/// Ограничение: все спрайты в одном вызове должны использовать одну текстуру.
/// Для нескольких текстур — либо несколько вызовов, либо texture array (сложнее).
///
/// Использование:
///   renderer.Begin();
///   renderer.Add(transform, sprite);   // для каждой сущности
///   renderer.End();                    // flush одним draw call
/// </summary>
public sealed class InstancedRenderer : IDisposable
{
    private const int MaxInstances = 65536;

    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao;
    private readonly uint _quadVbo;
    private readonly uint _ebo;
    private readonly uint _instanceVbo;

    private readonly InstanceData[] _instances = new InstanceData[MaxInstances];
    private int _count;
    private Texture2D? _currentTexture;
    private Matrix4x4 _projection;

    public int InstanceCount => _count;

    public unsafe InstancedRenderer(GL gl)
    {
        _gl = gl;

        const string vs = @"#version 330 core

// Per-vertex (quad)
layout(location = 0) in vec2 aVertexPos;
layout(location = 1) in vec2 aVertexUV;

// Per-instance
layout(location = 2) in vec2 iPosition;
layout(location = 3) in vec2 iSize;
layout(location = 4) in vec4 iColor;
layout(location = 5) in vec2 iUVMin;
layout(location = 6) in vec2 iUVMax;
layout(location = 7) in float iRotation;

uniform mat4 uProjection;

out vec2 vUV;
out vec4 vColor;

void main() {
    // Поворот вершины вокруг центра
    vec2 localPos = (aVertexPos - 0.5) * iSize;
    float c = cos(iRotation);
    float s = sin(iRotation);
    vec2 rotated = vec2(
        localPos.x * c - localPos.y * s,
        localPos.x * s + localPos.y * c
    );
    vec2 worldPos = iPosition + rotated;

    vUV = mix(iUVMin, iUVMax, aVertexUV);
    vColor = iColor;

    gl_Position = uProjection * vec4(worldPos, 0.0, 1.0);
}";

        const string fs = @"#version 330 core
in vec2 vUV;
in vec4 vColor;
uniform sampler2D uTexture;
out vec4 FragColor;
void main() {
    FragColor = texture(uTexture, vUV) * vColor;
}";

        _shader = new Shader(gl, vs, fs);

        // Quad: 4 вершины (0..1)
        float[] quad = {
            // x, y, u, v
            0f, 0f, 0f, 0f,
            1f, 0f, 1f, 0f,
            1f, 1f, 1f, 1f,
            0f, 1f, 0f, 1f,
        };

        uint[] indices = { 0, 1, 2, 2, 3, 0 };

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Quad VBO
        _quadVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _quadVbo);
        fixed (float* p = quad)
            _gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(quad.Length * sizeof(float)), p, BufferUsageARB.StaticDraw);

        // Index buffer
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* p = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);

        // Атрибуты вершины
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
            4 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
            4 * sizeof(float), (void*)(2 * sizeof(float)));

        // Instance VBO
        _instanceVbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(MaxInstances * sizeof(InstanceData)), null, BufferUsageARB.DynamicDraw);

        int stride = sizeof(InstanceData);

        // Атрибут 2: Position (Vector2)
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)0);
        _gl.VertexAttribDivisor(2, 1);

        // Атрибут 3: Size (Vector2)
        _gl.EnableVertexAttribArray(3);
        _gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)8);
        _gl.VertexAttribDivisor(3, 1);

        // Атрибут 4: Color (Vector4)
        _gl.EnableVertexAttribArray(4);
        _gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)16);
        _gl.VertexAttribDivisor(4, 1);

        // Атрибут 5: UVMin (Vector2)
        _gl.EnableVertexAttribArray(5);
        _gl.VertexAttribPointer(5, 2, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)32);
        _gl.VertexAttribDivisor(5, 1);

        // Атрибут 6: UVMax (Vector2)
        _gl.EnableVertexAttribArray(6);
        _gl.VertexAttribPointer(6, 2, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)40);
        _gl.VertexAttribDivisor(6, 1);

        // Атрибут 7: Rotation (float)
        _gl.EnableVertexAttribArray(7);
        _gl.VertexAttribPointer(7, 1, VertexAttribPointerType.Float, false,
            (uint)stride, (void*)48);
        _gl.VertexAttribDivisor(7, 1);
    }

    /// <summary>Начать новый батч. Должен быть вызван перед Add.</summary>
    public void Begin(Matrix4x4 projection, Texture2D texture)
    {
        _projection = projection;
        _currentTexture = texture;
        _count = 0;
    }

    /// <summary>Добавить спрайт в текущий батч.</summary>
    public void Add(Transform transform, Sprite sprite, float rotation = 0f)
    {
        if (_count >= MaxInstances) return;
        if (sprite.Texture != _currentTexture) return; // разные текстуры — пропускаем
        if (!sprite.Enabled) return;

        Vector2 uvMin = Vector2.Zero;
        Vector2 uvMax = Vector2.One;

        if (sprite.SourceRect.HasValue)
        {
            var r = sprite.SourceRect.Value;
            uvMin = new Vector2(r.X / (float)sprite.Texture!.Width,
                                r.Y / (float)sprite.Texture.Height);
            uvMax = new Vector2(r.Right / (float)sprite.Texture.Width,
                                r.Bottom / (float)sprite.Texture.Height);
        }

        _instances[_count++] = new InstanceData
        {
            Position = transform.Position,
            Size = sprite.Size,
            Color = sprite.Color,
            UVMin = uvMin,
            UVMax = uvMax,
            Rotation = rotation
        };
    }

    /// <summary>Выполнить draw call — нарисовать все накопленные экземпляры.</summary>
    public unsafe void End()
    {
        if (_count == 0 || _currentTexture == null) return;

        _shader.Use();
        _shader.SetMatrix4("uProjection", _projection);
        _shader.SetInt("uTexture", 0);

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVbo);

        fixed (InstanceData* p = _instances)
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(_count * sizeof(InstanceData)), p);

        _currentTexture.Bind(0);

        _gl.DrawElementsInstanced(
            PrimitiveType.Triangles,
            6,
            DrawElementsType.UnsignedInt,
            null,
            (uint)_count);

        _count = 0;
    }

    public void Dispose()
    {
        _shader.Dispose();
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_quadVbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteBuffer(_instanceVbo);
    }
}