using MyEngine.Math;
using MyEngine.Rendering;
using MyEngine.Rendering.Effects;
using System.Numerics;

namespace MyEngine.Components;

public sealed class Sprite
{
    public Vector2 Size;
    public Vector4 Color = Vector4.One;
    public int Layer;

    public Texture2D? Texture;
    public IntRect? SourceRect;

    /// <summary>
    /// Рисовать ли спрайт в этом кадре. Игровые системы могут
    /// временно отключать — например, для мигания при i-frames.
    /// </summary>
    public bool Enabled = true;

    /// <summary>Какой эффект применяется. None — обычный рендер.</summary>
    public SpriteEffect EffectType = SpriteEffect.None;

    /// <summary>Параметры эффекта. Значение зависит от EffectType.</summary>
    public Vector4 EffectParams = Vector4.Zero;
}