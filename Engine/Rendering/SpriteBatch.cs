using MyEngine.Rendering.Effects;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MyEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal struct SpriteVertex
{
    public Vector2 Position;   // 8 байт
    public Vector2 UV;         // 8 байт
    public Vector4 Color;      // 16 байт
    public float EffectType;   // 4 байта
    public float _pad0;        // 4 байта padding
    public Vector4 EffectParams; // 16 байт
}

public sealed class SpriteBatch : IDisposable
{
    private const int MaxSprites = 2048;
    private readonly GL _gl;
    private readonly uint _vao, _vbo, _ebo;
    private readonly SpriteVertex[] _vertices = new SpriteVertex[MaxSprites * 4];
    private readonly Shader _shader;
    private readonly Texture2D _white;
    private int _spriteCount;
    private Texture2D? _current;

    private const string VertexSrc = @"#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aUV;
layout(location = 2) in vec4 aColor;
layout(location = 3) in float aEffectType;
layout(location = 4) in vec4 aEffectParams;

uniform mat4 uProjection;

out vec2 vUV;
out vec4 vColor;
flat out int vEffectType;
flat out vec4 vEffectParams;

void main() {
    vUV = aUV;
    vColor = aColor;
    vEffectType = int(aEffectType);
    vEffectParams = aEffectParams;
    gl_Position = uProjection * vec4(aPos, 0.0, 1.0);
}";

    private const string FragmentSrc = @"#version 330 core
in vec2 vUV;
in vec4 vColor;
flat in int vEffectType;
flat in vec4 vEffectParams;

uniform sampler2D uTexture;
uniform vec2 uTexelSize;  // 1/width, 1/height текстуры

out vec4 FragColor;

const int EFFECT_NONE = 0;
const int EFFECT_FLASH = 1;
const int EFFECT_OUTLINE = 2;
const int EFFECT_DISSOLVE = 3;
const int EFFECT_WAVE = 4;
const int EFFECT_GRAYSCALE = 5;

// Простой 2D-шум для dissolve
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() {
    vec2 uv = vUV;

    // === WAVE: сдвиг UV ===
    if (vEffectType == EFFECT_WAVE) {
        float amp = vEffectParams.x;
        float freq = vEffectParams.y;
        float speed = vEffectParams.z;
        float time = vEffectParams.w;
        uv.x += sin(vUV.y * freq + time * speed) * amp;
        uv.y += cos(vUV.x * freq + time * speed * 0.7) * amp * 0.5;
    }

    vec4 texColor = texture(uTexture, uv);

    // === OUTLINE: обводка ===
    if (vEffectType == EFFECT_OUTLINE) {
        float thickness = vEffectParams.x;
        vec3 outlineColor = vEffectParams.yzw;

        // Альфа по 4 направлениям
        float aL = texture(uTexture, vUV - vec2(thickness, 0.0)).a;
        float aR = texture(uTexture, vUV + vec2(thickness, 0.0)).a;
        float aU = texture(uTexture, vUV - vec2(0.0, thickness)).a;
        float aD = texture(uTexture, vUV + vec2(0.0, thickness)).a;
        float neighbors = max(max(aL, aR), max(aU, aD));

        // Если пиксель вне спрайта, но рядом есть спрайт — рисуем обводку
        float outside = 1.0 - texColor.a;
        float outline = outside * neighbors;

        // Смешиваем: обводка + спрайт
        vec4 result = texColor;
        result.rgb = mix(outlineColor, texColor.rgb, texColor.a);
        result.a = max(texColor.a, outline);
        FragColor = result * vColor;
        return;
    }

    // === DISSOLVE: шумное исчезновение ===
    if (vEffectType == EFFECT_DISSOLVE) {
        float progress = vEffectParams.x;
        vec3 edgeColor = vEffectParams.yzw;

        float noise = hash(floor(vUV * 200.0));  // пиксельный шум
        // progress = 0: ничего не удалено
        // progress = 1: всё удалено
        if (noise < progress) discard;

        // Край — тонкая полоса, где noise близок к progress
        float edge = smoothstep(progress, progress + 0.1, noise);
        vec3 col = mix(edgeColor, texColor.rgb, edge);
        FragColor = vec4(col, texColor.a) * vColor;
        return;
    }

    // === GRAYSCALE: ч/б ===
    if (vEffectType == EFFECT_GRAYSCALE) {
        float amount = vEffectParams.x;
        float gray = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
        vec3 col = mix(texColor.rgb, vec3(gray), amount);
        FragColor = vec4(col, texColor.a) * vColor;
        return;
    }

    // === FLASH: вспышка цветом ===
    if (vEffectType == EFFECT_FLASH) {
        float strength = vEffectParams.x;
        vec3 flashColor = vEffectParams.yzw;
        vec3 col = mix(texColor.rgb, flashColor, strength);
        FragColor = vec4(col, texColor.a) * vColor;
        return;
    }

    // === NONE (по умолчанию) ===
    FragColor = texColor * vColor;
}";

    public unsafe SpriteBatch(GL gl)
    {
        _gl = gl;
        _shader = new Shader(gl, VertexSrc, FragmentSrc);
        _white = Texture2D.White(gl);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        _gl.BufferData(BufferTargetARB.ArrayBuffer,
            (nuint)(_vertices.Length * sizeof(SpriteVertex)), null, BufferUsageARB.DynamicDraw);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        uint[] indices = new uint[MaxSprites * 6];
        for (int i = 0; i < MaxSprites; i++)
        {
            uint o = (uint)(i * 4);
            indices[i * 6 + 0] = o + 0;
            indices[i * 6 + 1] = o + 1;
            indices[i * 6 + 2] = o + 2;
            indices[i * 6 + 3] = o + 2;
            indices[i * 6 + 4] = o + 3;
            indices[i * 6 + 5] = o + 0;
        }
        fixed (uint* ip = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(indices.Length * sizeof(uint)), ip, BufferUsageARB.StaticDraw);

        int stride = sizeof(SpriteVertex);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)8);
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)16);

        _gl.EnableVertexAttribArray(3);
        _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, (uint)stride, (void*)32);
        _gl.EnableVertexAttribArray(4);
        _gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)40);
    }

    public void Begin(Matrix4x4 projection)
    {
        // Blend нужен для alpha-текстур (шрифты, полупрозрачные спрайты).
        // Постобработка и UIRoundedRenderer могут отключать blend,
        // поэтому включаем его здесь явно — чтобы не зависеть от порядка рендера.
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();
        _shader.SetMatrix4("uProjection", projection);
        _shader.SetInt("uTexture", 0);
        _spriteCount = 0;
        _current = null;
    }

    public void DrawRect(Vector2 position, Vector2 size, Vector4 color)
    => Draw(_white, position, size, Vector2.Zero, Vector2.One, 0f, new Vector2(0.5f), color);

    public void Draw(Texture2D texture, Vector2 position, Vector2 size,
                     float rotation = 0f, Vector2? origin = null, Vector4? color = null,
                     SpriteEffect effect = SpriteEffect.None, Vector4? effectParams = null)
        => Draw(texture, position, size, Vector2.Zero, Vector2.One, rotation, origin, color, effect, effectParams);

    public void Draw(Texture2D texture, Vector2 position, Vector2 size,
                     Vector2 uvMin, Vector2 uvMax,
                     float rotation = 0f, Vector2? origin = null, Vector4? color = null,
                     SpriteEffect effect = SpriteEffect.None, Vector4? effectParams = null)
    {
        if (_current != null && _current != texture) Flush();
        _current = texture;
        if (_spriteCount >= MaxSprites) Flush();

        var o = origin ?? new Vector2(0.5f);
        var c = color ?? Vector4.One;
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        Span<Vector2> corners = stackalloc Vector2[4]
        {
        new(-o.X * size.X,      -o.Y * size.Y),
        new((1 - o.X) * size.X, -o.Y * size.Y),
        new((1 - o.X) * size.X, (1 - o.Y) * size.Y),
        new(-o.X * size.X,      (1 - o.Y) * size.Y),
    };

        Span<Vector2> uvs = stackalloc Vector2[4]
        {
        new(uvMin.X, uvMin.Y),
        new(uvMax.X, uvMin.Y),
        new(uvMax.X, uvMax.Y),
        new(uvMin.X, uvMax.Y),
    };

        float effectFloat = (float)(int)effect;
        Vector4 paramsVec = effectParams ?? Vector4.Zero;

        int idx = _spriteCount * 4;
        for (int i = 0; i < 4; i++)
        {
            var p = corners[i];
            var rp = new Vector2(p.X * cos - p.Y * sin, p.X * sin + p.Y * cos);
            _vertices[idx + i] = new SpriteVertex
            {
                Position = position + rp,
                UV = uvs[i],
                Color = c,
                EffectType = effectFloat,     // ← NEW
                _pad0 = 0,                     // ← NEW (padding, всегда 0)
                EffectParams = paramsVec       // ← NEW
            };
        }


        _spriteCount++;
    }

    public void End() => Flush();

    private unsafe void Flush()
    {
        if (_spriteCount == 0) return;
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (SpriteVertex* p = _vertices)
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,
                (nuint)(_spriteCount * 4 * sizeof(SpriteVertex)), p);
        (_current ?? _white).Bind(0);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)(_spriteCount * 6),
            DrawElementsType.UnsignedInt, (void*)0);
        _spriteCount = 0;
        _current = null;
    }

    public void Dispose()
    {
        _shader.Dispose();
        _white.Dispose();
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteVertexArray(_vao);
    }
}