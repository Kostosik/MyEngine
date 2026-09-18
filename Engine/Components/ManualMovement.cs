namespace MyEngine.Components;

/// <summary>
/// Маркер: сущность двигается вручную (не через MovementSystem).
/// Используется для игрока, которого двигает UpdateVariable ради отзывчивости,
/// или для kinematic-объектов, которыми управляет скрипт.
/// </summary>
public sealed class ManualMovement { }