using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Описание одного символа в атласе шрифта.
/// </summary>
public struct Glyph
{
    /// <summary>UV-координаты верхнего левого угла глифа в атласе.</summary>
    public Vector2 UV0;

    /// <summary>UV-координаты нижнего правого угла глифа в атласе.</summary>
    public Vector2 UV1;

    /// <summary>Размер глифа в пикселях.</summary>
    public Vector2 Size;

    /// <summary>
    /// Смещение от pen-позиции до верхнего левого угла глифа.
    /// X — вправо, Y — обычно отрицательный (глиф поднимается над базовой линией).
    /// </summary>
    public Vector2 Offset;

    /// <summary>Горизонтальный шаг после отрисовки глифа (в пикселях).</summary>
    public float Advance;
}