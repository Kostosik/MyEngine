using System.Numerics;

namespace MyEngine.Components;

/// <summary>
/// Точечный источник света. Радиальный градиент от центра к краю.
///
/// Свет складывается в lightmap, потом умножается со сценой.
/// </summary>
public sealed class PointLight
{
    /// <summary>Цвет света. (1, 0.8, 0.4) — тёплый ламповый.</summary>
    public Vector3 Color = new(1f, 1f, 1f);

    /// <summary>Радиус действия в пикселях.</summary>
    public float Radius = 200f;

    /// <summary>Интенсивность (множитель цвета).</summary>
    public float Intensity = 1f;

    /// <summary>Мягкость затухания. 1 = линейно, 2 = квадратично.</summary>
    public float Falloff = 2f;

    /// <summary>
    /// Пульсация — множитель радиуса, плавно меняющийся.
    /// Если Pulsate=false, не используется.
    /// </summary>
    public bool Pulsate = false;

    /// <summary>Скорость пульсации.</summary>
    public float PulsateSpeed = 2f;

    /// <summary>Амплитуда пульсации (0..1).</summary>
    public float PulsateAmount = 0.15f;

    /// <summary>Внутренний таймер для пульсации.</summary>
    public float Time;
}