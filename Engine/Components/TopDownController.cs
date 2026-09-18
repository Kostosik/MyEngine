using MyEngine.Diagnostics.Validation;
using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Components;

[RequireComponent(typeof(Transform))]
/// <summary>
/// Управляемый сверху-вниз персонаж. Читает InputDirection (Vector2,
/// обычно нормализованный) и превращает его в Velocity.
///
/// Игра отвечает только за заполнение InputDirection — читает ввод,
/// пишет в компонент. Всё остальное — движок.
/// </summary>
public sealed class TopDownController
{
    [Positive]
    /// <summary>Базовая максимальная скорость (пиксели/сек).</summary>
    public float MaxSpeed = 220f;
    [Positive]
    /// <summary>Множитель скорости — можно менять баффами/дебаффами.</summary>
    public float SpeedMultiplier = 1f;

    /// <summary>
    /// Желаемое направление движения, задаётся игрой каждый кадр.
    /// Длина 0..1. Если 0 — персонаж стоит.
    /// </summary>
    public Vector2 InputDirection;
}