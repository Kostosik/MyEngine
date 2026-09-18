using MyEngine.Components;
using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Обрабатывает платформенное движение: гравитация, прыжок,
/// горизонтальное движение с ускорением, coyote time, jump buffer.
///
/// Работает в фиксированном апдейте — стабильнее и точнее.
/// Отзывчивость даёт jump buffer + coyote time.
///
/// Двигает Transform напрямую — CollisionSystem потом разрешает коллизии.
/// </summary>
public sealed class PlatformerControllerSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 20;

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<PlatformerController>())
        {
            var c = e.Get<PlatformerController>()!;
            var t = e.Get<Transform>();
            var v = e.Get<Velocity>();
            if (t == null || v == null) continue;

            // ============================================================
            // 1. Таймеры Coyote / Jump Buffer
            // ============================================================

            if (c.Grounded) c.TimeSinceGrounded = 0f;
            else c.TimeSinceGrounded += dt;

            if (c.JumpPressed)
            {
                c.JumpBufferTimer = c.JumpBufferTime;
                c.JumpPressed = false;
            }
            else if (c.JumpBufferTimer > 0f)
            {
                c.JumpBufferTimer -= dt;
            }

            // ============================================================
            // 2. Горизонтальное движение
            // ============================================================

            float targetX = c.InputX * c.MaxSpeedX;
            float accel;
            if (c.Grounded)
                accel = c.InputX != 0 ? c.Acceleration : c.GroundDeceleration;
            else
                accel = c.Acceleration * c.AirControl;

            // Плавное движение к targetX
            if (v.Value.X < targetX)
                v.Value.X = MathF.Min(targetX, v.Value.X + accel * dt);
            else if (v.Value.X > targetX)
                v.Value.X = MathF.Max(targetX, v.Value.X - accel * dt);

            // ============================================================
            // 3. Прыжок
            // ============================================================

            bool canJump = c.Grounded || c.TimeSinceGrounded <= c.CoyoteTime;

            if (c.JumpBufferTimer > 0f && canJump)
            {
                v.Value.Y = -c.JumpForce; // вверх (в экранных координатах Y растёт вниз)
                c.JumpBufferTimer = 0f;
                c.IsJumping = true;
                c.TimeSinceGrounded = c.CoyoteTime + 1f; // запрет двойного прыжка
            }

            // Variable jump height — отпустил рано, прыжок короче
            if (c.IsJumping && !c.JumpHeld && v.Value.Y < 0f)
            {
                v.Value.Y *= c.JumpCutMultiplier;
                c.IsJumping = false;
            }

            if (v.Value.Y >= 0f) c.IsJumping = false;

            // ============================================================
            // 4. Гравитация
            // ============================================================

            float gravityMultiplier = v.Value.Y < 0f ? c.GravityUp : c.GravityDown;
            v.Value.Y += 1500f * gravityMultiplier * dt;

            // Ограничение скорости падения
            if (v.Value.Y > c.MaxFallSpeed)
                v.Value.Y = c.MaxFallSpeed;

            // ============================================================
            // 5. Применение скорости
            // ============================================================

            // Применяем скорость к позиции. CollisionSystem разрешит коллизии
            // в этом же кадре (в режиме Platformer).
            t.Position += v.Value * dt;
        }
    }
}