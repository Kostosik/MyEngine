using MyEngine.Diagnostics.Validation;
using System.Numerics;

namespace MyEngine.Components;

/// <summary>
/// Управляемый сбоку-персонаж (платформер).
/// Работает в паре с PlatformerControllerSystem и CollisionSystem в режиме Platformer.
///
/// Игра заполняет InputDirection.X (влево/вправо) и JumpHeld/JumpPressed.
/// Всё остальное — движок: гравитация, прыжок, coyote time, jump buffer.
/// </summary>
public sealed class PlatformerController
{
    // ============================================================
    // Настройки движения
    // ============================================================
    [Positive]
    /// <summary>Максимальная скорость по X.</summary>
    public float MaxSpeedX = 220f;
    [Positive]
    /// <summary>Ускорение при движении.</summary>
    public float Acceleration = 1800f;
    [Positive]
    /// <summary>Торможение при отпускании клавиши (на земле).</summary>
    public float GroundDeceleration = 2200f;
    [Positive]
    /// <summary>Управление в воздухе (множитель 0..1).</summary>
    public float AirControl = 0.5f;

    // ============================================================
    // Настройки прыжка
    // ============================================================
    [Positive]
    /// <summary>Начальная скорость прыжка (пиксели/сек). Больше — выше прыжок.</summary>
    public float JumpForce = 550f;
    [Positive]
    /// <summary>Множитель гравитации при движении вверх.</summary>
    public float GravityUp = 1f;
    [Positive]
    /// <summary>Множитель гравитации при падении. Больше 1 — быстрее падаешь.</summary>
    public float GravityDown = 1.5f;
    [Positive]
    /// <summary>Максимальная скорость падения.</summary>
    public float MaxFallSpeed = 900f;
    [Positive]
    /// <summary>
    /// При отпускании прыжка в полёте скорость Y обрезается до этой доли.
    /// Даёт «переменную высоту» прыжка — держишь дольше, прыгаешь выше.
    /// </summary>
    public float JumpCutMultiplier = 0.5f;
    [Positive]
    /// <summary>
    /// «Coyote time» — сколько секунд после схода с края можно ещё прыгнуть.
    /// 0.1 сек — ощущается как «прощает ошибки».
    /// </summary>
    public float CoyoteTime = 0.1f;
    [Positive]
    /// <summary>
    /// «Jump buffer» — сколько секунд после нажатия прыжка можно засчитать его при приземлении.
    /// 0.1 сек — нажал чуть раньше, прыжок сработает.
    /// </summary>
    public float JumpBufferTime = 0.1f;

    // ============================================================
    // Runtime-состояние (обновляет система)
    // ============================================================

    /// <summary>Направление по X (заполняет PlayerInputSystem).</summary>
    public float InputX;

    /// <summary>Кнопка прыжка удерживается.</summary>
    public bool JumpHeld;

    /// <summary>Кнопка прыжка нажата в этом кадре (для jump buffer).</summary>
    public bool JumpPressed;

    /// <summary>Стоит ли на земле. Обновляет CollisionSystem в Platformer-режиме.</summary>
    public bool Grounded;

    /// <summary>Сколько секунд прошло с момента схода с земли.</summary>
    public float TimeSinceGrounded;

    /// <summary>Оставшееся время буфера прыжка. Больше 0 — прыжок «запомнен».</summary>
    public float JumpBufferTimer;

    /// <summary>Идёт ли прыжок (для variable jump height).</summary>
    public bool IsJumping;
}