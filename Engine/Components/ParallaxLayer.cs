using System.Numerics;

namespace MyEngine.Components;

/// <summary>
/// Слой параллакса. Прикрепляется к фоновым объектам.
///
/// ParallaxSystem каждый кадр пересчитывает Transform.Position:
///   position = BasePosition + camera.Position * (1 - ParallaxFactor)
///
/// При factor = 1.0 — объект движется вместе с миром (не отстаёт).
/// При factor = 0.5 — отстаёт вдвое.
/// При factor = 0.0 — не двигается (небо).
///
/// BasePosition — «якорная» позиция, вокруг которой строится слой.
/// Обычно это позиция в мире, где слой стоит, когда камера в начале координат.
/// </summary>
public sealed class ParallaxLayer
{
    /// <summary>0 = небо (не двигается), 1 = обычный мир, 0.5 = средний план.</summary>
    public float ParallaxFactor = 0.5f;

    /// <summary>Базовая позиция в мире. Слой «колеблется» вокруг неё.</summary>
    public Vector2 BasePosition;

    /// <summary>
    /// Дополнительное смещение поверх формулы.
    /// Используется для ручного выравнивания слоя.
    /// </summary>
    public Vector2 Offset;
}