using System.Numerics;
using MyEngine.Math;

namespace MyEngine.Rendering;

/// <summary>
/// 2D-камера с deadzone, look-ahead, границами и эффектом тряски.
///
/// Позиция камеры — центр обзора в мировых координатах.
/// После вызова Follow(...) учитываются все настройки.
/// Shake добавляет визуальный сдвиг, не меняя логическую Position.
/// </summary>
public sealed class Camera2D
{
    // --- Базовые ---
    public Vector2 Position = Vector2.Zero;
    public float Zoom = 1f;

    // --- Deadzone ---
    /// <summary>
    /// Размер центральной зоны, внутри которой цель может двигаться,
    /// не сдвигая камеру. (0, 0) — камера всегда строго на цели.
    /// </summary>
    public Vector2 DeadzoneSize = Vector2.Zero;

    // --- Look-ahead ---
    /// <summary>
    /// Сдвиг камеры в сторону движения цели (пиксели).
    /// 0 — без сдвига. Положительное значение даёт «упреждение».
    /// </summary>
    public Vector2 LookAheadDistance = Vector2.Zero;

    /// <summary>Скорость сглаживания look-ahead. 0 = отключено.</summary>
    public float LookAheadSmoothing = 4f;

    // --- Плавность следования ---
    /// <summary>
    /// Коэффициент сглаживания движения камеры.
    /// 0 — мгновенно. 5–8 — типично для платформеров. 15+ — почти мгновенно.
    /// </summary>
    public float FollowSmoothing = 0f;

    // --- Границы мира ---
    /// <summary>
    /// Ограничения по краям. Null — без ограничений.
    /// Камера не выйдет за эти границы с учётом половины обзора.
    /// </summary>
    public Aabb? Bounds;

    // --- Shake ---
    private float _shakeIntensity;
    private float _shakeDuration;
    private float _shakeTime;
    private Vector2 _shakeOffset;
    private float _shakeSeed;

    // --- Внутреннее ---
    private Vector2 _lookAheadCurrent;

    /// <summary>Текущее смещение от тряски (не влияет на Position).</summary>
    public Vector2 ShakeOffset => _shakeOffset;

    /// <summary>
    /// Плавно следит за целью с учётом deadzone, look-ahead и границ.
    /// Вызывать каждый кадр в UpdateVariable.
    /// </summary>
    public void Follow(Vector2 targetPos, Vector2 targetVelocity, float dt)
    {
        // 1. Look-ahead
        Vector2 lookAhead = Vector2.Zero;
        if (LookAheadDistance != Vector2.Zero && targetVelocity.LengthSquared() > 1f)
        {
            var dir = Vector2.Normalize(targetVelocity);
            lookAhead = dir * LookAheadDistance;
        }

        if (LookAheadSmoothing > 0f)
        {
            float a = 1f - MathF.Exp(-LookAheadSmoothing * dt);
            _lookAheadCurrent = Vector2.Lerp(_lookAheadCurrent, lookAhead, a);
        }
        else
        {
            _lookAheadCurrent = lookAhead;
        }

        var desired = targetPos + _lookAheadCurrent;

        // 2. Deadzone — сдвигаем камеру, только если цель вышла за центр
        if (DeadzoneSize != Vector2.Zero)
        {
            var half = DeadzoneSize * 0.5f;
            var min = Position - half;
            var max = Position + half;

            if (desired.X < min.X) Position.X = desired.X + half.X;
            else if (desired.X > max.X) Position.X = desired.X - half.X;

            if (desired.Y < min.Y) Position.Y = desired.Y + half.Y;
            else if (desired.Y > max.Y) Position.Y = desired.Y - half.Y;
        }
        else
        {
            Position = desired;
        }

        // 3. Плавность
        if (FollowSmoothing > 0f && DeadzoneSize == Vector2.Zero)
        {
            float a = 1f - MathF.Exp(-FollowSmoothing * dt);
            Position = Vector2.Lerp(Position, desired, a);
        }
    }

    /// <summary>
    /// Применить границы обзора. Нужно вызывать после Follow,
    /// но требует screenWidth/Height — обычно из Application.
    /// </summary>
    public void ApplyBounds(int screenWidth, int screenHeight)
    {
        if (Bounds == null) return;

        float halfW = screenWidth / (2f * Zoom);
        float halfH = screenHeight / (2f * Zoom);

        var b = Bounds.Value;
        float minX = b.Min.X + halfW;
        float maxX = b.Max.X - halfW;
        float minY = b.Min.Y + halfH;
        float maxY = b.Max.Y - halfH;

        if (minX > maxX) Position.X = (b.Min.X + b.Max.X) * 0.5f;
        else Position.X = System.Math.Clamp(Position.X, minX, maxX);

        if (minY > maxY) Position.Y = (b.Min.Y + b.Max.Y) * 0.5f;
        else Position.Y = System.Math.Clamp(Position.Y, minY, maxY);
    }

    /// <summary>
    /// Запустить тряску. Если тряска уже идёт — интенсивность
    /// берётся как максимум из текущей и новой.
    /// </summary>
    public void Shake(float intensity, float duration)
    {
        _shakeIntensity = System.Math.Max(_shakeIntensity, intensity);
        _shakeDuration = duration;
        _shakeTime = duration;
        _shakeSeed = (float)Random.Shared.NextDouble() * 1000f;
    }

    /// <summary>Обновить таймер тряски. Вызывать каждый кадр.</summary>
    public void UpdateShake(float dt)
    {
        if (_shakeTime <= 0f)
        {
            _shakeOffset = Vector2.Zero;
            return;
        }

        _shakeTime -= dt;

        float t = _shakeTime / _shakeDuration;
        float intensity = _shakeIntensity * t;

        // Псевдослучайная тряска через sin с разными частотами
        float sx = MathF.Sin((_shakeTime * 47f + _shakeSeed) * 1.7f);
        float sy = MathF.Cos((_shakeTime * 61f + _shakeSeed) * 2.3f);

        _shakeOffset = new Vector2(sx, sy) * intensity;

        if (_shakeTime <= 0f) _shakeOffset = Vector2.Zero;
    }

    /// <summary>
    /// Проекция с учётом тряски. Используется рендером.
    /// </summary>
    public Matrix4x4 GetProjection(int screenWidth, int screenHeight)
    {
        var pos = Position + _shakeOffset;
        float halfW = screenWidth / (2f * Zoom);
        float halfH = screenHeight / (2f * Zoom);
        float left = pos.X - halfW;
        float right = pos.X + halfW;
        float top = pos.Y - halfH;
        float bottom = pos.Y + halfH;
        return Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, -1f, 1f);
    }

    public Vector2 WorldToScreen(Vector2 world, float screenWidth, float screenHeight)
    {
        var pos = Position + _shakeOffset;
        float halfW = screenWidth / (2f * Zoom);
        float halfH = screenHeight / (2f * Zoom);
        float left = pos.X - halfW;
        float top = pos.Y - halfH;
        float sx = (world.X - left) / (2f * halfW) * screenWidth;
        float sy = (world.Y - top) / (2f * halfH) * screenHeight;
        return new Vector2(sx, sy);
    }

    /// <summary>
    /// Экранные координаты (в пикселях окна) → мировые.
    /// Учитывает zoom, shake и позицию камеры.
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screen, float screenWidth, float screenHeight)
    {
        var pos = Position + _shakeOffset;
        float halfW = screenWidth / (2f * Zoom);
        float halfH = screenHeight / (2f * Zoom);
        float left = pos.X - halfW;
        float top = pos.Y - halfH;
        float wx = left + (screen.X / screenWidth) * (2f * halfW);
        float wy = top + (screen.Y / screenHeight) * (2f * halfH);
        return new Vector2(wx, wy);
    }
}