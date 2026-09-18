namespace MyEngine.Rendering.Effects;

/// <summary>
/// Тип визуального эффекта, применяемого к спрайту.
/// Управляется полем Sprite.EffectType.
/// </summary>
public enum SpriteEffect
{
    /// <summary>Обычный рендер. Параметры не используются.</summary>
    None = 0,

    /// <summary>
    /// Вспышка — спрайт смешивается с цветом.
    /// Params.x = сила (0..1). Params.yzw = цвет вспышки (RGB).
    /// </summary>
    Flash = 1,

    /// <summary>
    /// Обводка вокруг спрайта.
    /// Params.x = толщина в UV (0.003 — тонко, 0.01 — толсто).
    /// Params.yzw = цвет обводки.
    /// </summary>
    Outline = 2,

    /// <summary>
    /// Растворение — спрайт исчезает с шумным порогом.
    /// Params.x = прогресс (0 = целый, 1 = исчез).
    /// Params.yzw = цвет «края» (обычно оранжевый/красный).
    /// </summary>
    Dissolve = 3,

    /// <summary>
    /// Волновое искажение UV.
    /// Params.x = амплитуда в UV (0.01), Params.y = частота, Params.z = скорость.
    /// </summary>
    Wave = 4,

    /// <summary>
    /// Ч/б.
    /// Params.x = сила (0 = цвет, 1 = серый).
    /// </summary>
    Grayscale = 5,
}