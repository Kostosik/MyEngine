using MyEngine.Math;
using MyEngine.Rendering;

namespace MyEngine.Animation;

/// <summary>
/// Описание анимации: набор кадров + скорость + зацикленность.
/// Кадры — это регионы на одной текстуре (спрайт-листе).
/// </summary>
public sealed class AnimationEngine
{
    /// <summary>
    /// События на конкретных кадрах.
    /// Ключ — индекс кадра, значение — строковый тег.
    /// Когда анимация переходит на этот кадр, публикуется AnimationEvent.
    /// </summary>
    public Dictionary<int, string> FrameEvents = new();

    public string Name = "";

    /// <summary>Текстура-источник. Все кадры — регионы на ней.</summary>
    public Texture2D Texture = null!;

    /// <summary>Регионы кадров в порядке воспроизведения.</summary>
    public IntRect[] Frames = System.Array.Empty<IntRect>();

    /// <summary>Кадров в секунду.</summary>
    public float Fps = 10f;

    /// <summary>Зациклена ли анимация. Если false — по завершении вызывается OnComplete.</summary>
    public bool Loop = true;

    public int FrameCount => Frames.Length;

    /// <summary>Общая длительность одного проигрывания в секундах.</summary>
    public float Duration => Frames.Length > 0 ? Frames.Length / Fps : 0f;
}