using MyEngine.Diagnostics;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MyEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal struct UIVertex
{
    public Vector2 Position;    // 8 байт
    public Vector2 Local;       // 8 байт — координата пикселя внутри прямоугольника
    public Vector2 Size;        // 8 байт — размер прямоугольника
    public float Radius;        // 4 байта
    public float _pad;          // 4 байта
    public Vector4 ColorTop;    // 16 байт
    public Vector4 ColorBottom; // 16 байт
    // Итого: 64 байта
}

/// <summary>
/// Рендер скруглённых прямоугольников для UI.
/// Один draw call может нарисовать тысячи прямоугольников —
/// все данные собираются в буфер и отправляются разом.
///
/// Особенности:
///   - скруглённые углы (радиус в пикселях)
///   - вертикальный градиент (ColorTop → ColorBottom)
///   - антиалиасинг на краях через smoothstep
///   - тень рисуется как ещё один скруглённый прямоугольник
///
/// Использование:
///   _rounded.Begin(projection);
///   _rounded.DrawRoundedRect(pos, size, colorTop, colorBottom, radius);
///   _rounded.End();
/// </summary>
public sealed class UIRoundedRenderer : IDisposable
{
    private const int MaxRects = 4096;
    private const int VertsPerRect = 4;
    private const int IndicesPerRect = 6;

    private readonly GL _gl;
    private readonly Shader _shader;
    private readonly uint _vao, _vbo, _ebo;
    private readonly UIVertex[] _vertices = new UIVertex[MaxRects * VertsPerRect];
    private int _rectCount;

    private const string VertexSrc = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aLocal;
layout(location = 2) in vec2 aSize;
layout(location = 3) in float aRadius;
layout(location = 4) in vec4 aColorTop;
layout(location = 5) in vec4 aColorBottom;

uniform mat4 uProjection;

out vec2 vLocal;
out vec2 vSize;
out float vRadius;
out vec4 vColorTop;
out vec4 vColorBottom;

void main() {
    vLocal = aLocal;
    vSize = aSize;
    vRadius = aRadius;
    vColorTop = aColorTop;
    vColorBottom = aColorBottom;
    gl_Position = uProjection * vec4(aPos, 0.0, 1.0);
}";

    private const string FragmentSrc = @"#version 330 core
in vec2 vLocal;
in vec2 vSize;
in float vRadius;
in vec4 vColorTop;
in vec4 vColorBottom;

out vec4 FragColor;

// Signed distance field для rounded box.
// Возвращает: отрицательное внутри, положительное снаружи, 0 на границе.
float roundedBoxSDF(vec2 p, vec2 b, float r) {
    vec2 q = abs(p) - b + r;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
}

void main() {
    vec2 center = vSize * 0.5;
    vec2 p = vLocal - center;

    float d = roundedBoxSDF(p, center, vRadius);

    // Antialiasing: пиксель с d в диапазоне [-1, 1] — на границе
    float alpha = 1.0 - smoothstep(-1.0, 1.0, d);

    // Вертикальный градиент
    float t = vLocal.y / max(vSize.y, 0.001);
    vec4 color = mix(vColorTop, vColorBottom, t);

    FragColor = vec4(color.rgb, color.a * alpha);
}";

    public UIRoundedRenderer(GL gl)
    {
        _gl = gl;
        _shader = new Shader(gl, VertexSrc, FragmentSrc);

        // Индексы для квадов (одинаковые для всех прямоугольников)
        uint[] indices = new uint[MaxRects * IndicesPerRect];
        for (int i = 0; i < MaxRects; i++)
        {
            uint o = (uint)(i * 4);
            indices[i * 6 + 0] = o + 0;
            indices[i * 6 + 1] = o + 1;
            indices[i * 6 + 2] = o + 2;
            indices[i * 6 + 3] = o + 2;
            indices[i * 6 + 4] = o + 3;
            indices[i * 6 + 5] = o + 0;
        }

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(_vertices.Length * sizeof(UIVertex)), null, BufferUsageARB.DynamicDraw);
        }

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        unsafe
        {
            fixed (uint* p = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                    (nuint)(indices.Length * sizeof(uint)), p, BufferUsageARB.StaticDraw);
        }

        int stride;
        unsafe
        {
            stride = sizeof(UIVertex);

            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)0);

            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)8);

            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)16);

            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)24);

            _gl.EnableVertexAttribArray(4);
            _gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)32);

            _gl.EnableVertexAttribArray(5);
            _gl.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false,
                (uint)stride, (void*)48);
        }
    }

    /// <summary>Начать батч. Все последующие DrawRoundedRect копятся.</summary>
    public void Begin(Matrix4x4 projection)
    {
        _rectCount = 0;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.ScissorTest);

        // Alpha blend — обычный для UI
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        _shader.SetMatrix4("uProjection", projection);
    }

    /// <summary>Нарисовать скруглённый прямоугольник с градиентом.</summary>
    public void DrawRoundedRect(
        Vector2 position, Vector2 size,
        Vector4 colorTop, Vector4 colorBottom,
        float radius = 0f)
    {
        if (size.X <= 0f || size.Y <= 0f)
            return;
        if (_rectCount >= MaxRects) Flush();

        // Ограничиваем радиус половиной меньшей стороны
        float maxRadius = MathF.Min(size.X, size.Y) * 0.5f;
        radius = System.Math.Clamp(radius, 0f, maxRadius);

        int idx = _rectCount * 4;

        _vertices[idx + 0] = MakeVertex(
            new Vector2(position.X, position.Y),
            new Vector2(0, 0), size, radius, colorTop, colorBottom);
        _vertices[idx + 1] = MakeVertex(
            new Vector2(position.X + size.X, position.Y),
            new Vector2(size.X, 0), size, radius, colorTop, colorBottom);
        _vertices[idx + 2] = MakeVertex(
            new Vector2(position.X + size.X, position.Y + size.Y),
            new Vector2(size.X, size.Y), size, radius, colorTop, colorBottom);
        _vertices[idx + 3] = MakeVertex(
            new Vector2(position.X, position.Y + size.Y),
            new Vector2(0, size.Y), size, radius, colorTop, colorBottom);

        _rectCount++;
    }

    /// <summary>Простой прямоугольник одним цветом без скругления.</summary>
    public void DrawRect(Vector2 position, Vector2 size, Vector4 color)
        => DrawRoundedRect(position, size, color, color, 0f);

    /// <summary>Скруглённый прямоугольник одним цветом.</summary>
    public void DrawRoundedRect(Vector2 position, Vector2 size, Vector4 color, float radius)
        => DrawRoundedRect(position, size, color, color, radius);

    /// <summary>Завершить батч и отправить всё на GPU.</summary>
    public void End()
    {
        Flush();
    }

    private unsafe void Flush()
    {
        if (_rectCount == 0) return;

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (UIVertex* p = _vertices)
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(_rectCount * 4 * sizeof(UIVertex)), p);

        _gl.DrawElements(PrimitiveType.Triangles,
            (uint)(_rectCount * IndicesPerRect),
            DrawElementsType.UnsignedInt, null);

        _rectCount = 0;
    }

    private static UIVertex MakeVertex(Vector2 pos, Vector2 local, Vector2 size,
        float radius, Vector4 top, Vector4 bottom)
    {
        return new UIVertex
        {
            Position = pos,
            Local = local,
            Size = size,
            Radius = radius,
            _pad = 0,
            ColorTop = top,
            ColorBottom = bottom
        };
    }

    public void Dispose()
    {
        _shader.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_vao);
    }
}