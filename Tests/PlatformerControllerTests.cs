using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Systems;
using System.Numerics;

namespace Tests;

public class PlatformerControllerTests
{
    // ============================================================
    // Вспомогательные методы
    // ============================================================

    private const float Dt = 1f / 60f;

    private static (World world, Entity entity, PlatformerController ctrl, Velocity vel)
        Setup(PlatformerController? ctrl = null)
    {
        var world = new World();
        var e = world.Create();
        var c = ctrl ?? new PlatformerController();
        var t = new Transform();
        var v = new Velocity();
        e.Add(c);
        e.Add(t);
        e.Add(v);
        return (world, e, c, v);
    }

    private static void Tick(World world, int count = 1)
    {
        var sys = new PlatformerControllerSystem();
        for (int i = 0; i < count; i++)
            sys.Update(world, Dt);
    }

    // ============================================================
    // Гравитация
    // ============================================================

    [Fact]
    public void GravityPullsDown()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 1f; // давно в воздухе

        Tick(world, 5);

        Assert.True(vel.Value.Y > 0, "Гравитация должна тянуть вниз (Y растёт)");
    }

    [Fact]
    public void GravityPullsDownWhenGrounded()
    {
        // Даже когда Grounded = true, гравитация применяется.
        // CollisionSystem потом обнуляет Velocity.Y.
        var (world, e, ctrl, vel) = Setup();
        ctrl.Grounded = true;

        Tick(world, 5);

        Assert.True(vel.Value.Y > 0, "Гравитация работает и на земле");
    }

    [Fact]
    public void MaxFallSpeedIsCapped()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.MaxFallSpeed = 300f;

        Tick(world, 100); // долго падаем

        Assert.True(vel.Value.Y <= ctrl.MaxFallSpeed,
            $"Скорость падения не должна превышать {ctrl.MaxFallSpeed}, получено {vel.Value.Y}");
    }

    // ============================================================
    // Прыжок
    // ============================================================

    [Fact]
    public void JumpWhenGrounded()
    {
        var (world, e, ctrl, vel) = Setup(new PlatformerController { JumpForce = 500f });
        ctrl.Grounded = true;
        ctrl.JumpPressed = true;

        Tick(world, 1);

        Assert.True(vel.Value.Y < 0, $"Прыжок должен толкнуть вверх, Y = {vel.Value.Y}");
    }

    [Fact]
    public void JumpForceAffectsSpeed()
    {
        var (world1, e1, c1, v1) = Setup(new PlatformerController { JumpForce = 300f });
        c1.Grounded = true;
        c1.JumpPressed = true;
        Tick(world1, 1);

        var (world2, e2, c2, v2) = Setup(new PlatformerController { JumpForce = 600f });
        c2.Grounded = true;
        c2.JumpPressed = true;
        Tick(world2, 1);

        Assert.True(v2.Value.Y < v1.Value.Y,
            "Больший JumpForce → сильнее отброс вверх (более отрицательный Y)");
    }

    [Fact]
    public void CannotJumpInAirWithoutCoyote()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 1f; // давно в воздухе, coyote кончился
        ctrl.JumpPressed = true;

        Tick(world, 1);

        // Y должен быть положительным (падаем), не отрицательным
        Assert.True(vel.Value.Y >= 0, "Прыжок в воздухе не срабатывает");
    }

    // ============================================================
    // Coyote time
    // ============================================================

    [Fact]
    public void CoyoteTime_AllowsJumpAfterLeavingGround()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.CoyoteTime = 0.1f;
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 0.05f; // 50 мс в воздухе — в пределах coyote
        ctrl.JumpPressed = true;

        Tick(world, 1);

        Assert.True(vel.Value.Y < 0, "Прыжок должен сработать в пределах coyote time");
    }

    [Fact]
    public void CoyoteTime_ExpiresAndBlocksJump()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.CoyoteTime = 0.1f;
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 0.2f; // 200 мс — уже за пределами
        ctrl.JumpPressed = true;

        Tick(world, 1);

        Assert.True(vel.Value.Y >= 0, "Прыжок не должен сработать после coyote time");
    }

    // ============================================================
    // Jump buffer
    // ============================================================

    [Fact]
    public void JumpBuffer_RemembersPressBeforeLanding()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.JumpBufferTime = 0.1f;
        ctrl.CoyoteTime = 0.1f;

        // Игрок в воздухе, нажимает прыжок заранее
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 1f; // далеко от земли
        ctrl.JumpPressed = true;

        Tick(world, 1);
        // Прыжка нет — игрок в воздухе

        Assert.True(vel.Value.Y >= 0, "Прыжок не сработал — игрок в воздухе");

        // Через мгновение — приземлился
        ctrl.Grounded = true;
        Tick(world, 1);

        Assert.True(vel.Value.Y < 0,
            "Jump buffer должен применить отложенное нажатие после приземления");
    }

    [Fact]
    public void JumpBuffer_Expires()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.JumpBufferTime = 0.1f;
        ctrl.CoyoteTime = 0.1f;

        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 1f;
        ctrl.JumpPressed = true;

        // Проходит больше времени, чем JumpBufferTime
        Tick(world, 10);

        ctrl.Grounded = true;
        vel.Value.Y = 0f;

        Tick(world, 1);

        Assert.True(vel.Value.Y >= 0, "Jump buffer истёк — прыжок не сработал");
    }

    // ============================================================
    // Variable jump height
    // ============================================================

    [Fact]
    public void JumpCut_ReducesSpeedWhenReleased()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.JumpCutMultiplier = 0.5f;
        ctrl.JumpForce = 500f;
        ctrl.Grounded = true;
        ctrl.JumpPressed = true;
        ctrl.JumpHeld = true;

        Tick(world, 1);
        float afterJump = vel.Value.Y; // ~ -500 + гравитация

        // Отпускаем прыжок
        ctrl.JumpHeld = false;
        Tick(world, 1);

        Assert.True(vel.Value.Y > afterJump,
            $"После отпускания скорость Y должна увеличиться (стать менее отрицательной). Было {afterJump}, стало {vel.Value.Y}");
    }

    [Fact]
    public void JumpNotCut_WhenHeld()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.JumpForce = 500f;
        ctrl.Grounded = true;
        ctrl.JumpPressed = true;
        ctrl.JumpHeld = true;

        Tick(world, 1);
        float y1 = vel.Value.Y;

        // Продолжаем держать
        Tick(world, 1);
        float y2 = vel.Value.Y;

        // Должны остаться в диапазоне "летим вверх" (Y отрицательный)
        Assert.True(y2 < 0, "Прыжок продолжается, пока держим Space");
    }

    // ============================================================
    // Горизонтальное движение
    // ============================================================

    [Fact]
    public void HorizontalAcceleration_ReachesMaxSpeed()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.MaxSpeedX = 220f;
        ctrl.Acceleration = 1800f;
        ctrl.Grounded = true;
        ctrl.InputX = 1f;

        Tick(world, 30); // достаточно времени для разгона

        Assert.True(MathF.Abs(vel.Value.X - ctrl.MaxSpeedX) < 1f,
            $"Скорость X должна достичь {ctrl.MaxSpeedX}, получено {vel.Value.X}");
    }

    [Fact]
    public void HorizontalDeceleration_StopsWhenNoInput()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.GroundDeceleration = 2200f;
        ctrl.Grounded = true;
        ctrl.InputX = 0f;
        vel.Value.X = 100f; // уже движемся

        Tick(world, 30);

        Assert.True(MathF.Abs(vel.Value.X) < 1f,
            $"При отсутствии ввода скорость должна упасть до 0, получено {vel.Value.X}");
    }

    [Fact]
    public void AirControl_IsSlowerThanGroundControl()
    {
        // На земле
        var (world1, e1, c1, v1) = Setup();
        c1.Acceleration = 1800f;
        c1.AirControl = 0.5f;
        c1.Grounded = true;
        c1.InputX = 1f;
        Tick(world1, 3);
        float groundSpeed = v1.Value.X;

        // В воздухе
        var (world2, e2, c2, v2) = Setup();
        c2.Acceleration = 1800f;
        c2.AirControl = 0.5f;
        c2.Grounded = false;
        c2.TimeSinceGrounded = 1f;
        c2.InputX = 1f;
        Tick(world2, 3);
        float airSpeed = v2.Value.X;

        Assert.True(airSpeed < groundSpeed,
            $"Управление в воздухе должно быть медленнее. Земля {groundSpeed}, воздух {airSpeed}");
    }

    [Fact]
    public void AirControlZero_NoHorizontalChangeInAir()
    {
        var (world, e, ctrl, vel) = Setup();
        ctrl.AirControl = 0f;
        ctrl.Grounded = false;
        ctrl.TimeSinceGrounded = 1f;
        ctrl.InputX = 1f;

        Tick(world, 10);

        Assert.Equal(0f, vel.Value.X);
    }

    // ============================================================
    // Позиция
    // ============================================================

    [Fact]
    public void PositionChangesByVelocity()
    {
        var world = new World();
        var e = world.Create();
        var ctrl = new PlatformerController();
        var t = new Transform { Position = new Vector2(0, 0) };
        var v = new Velocity { Value = new Vector2(100, 0) };
        e.Add(ctrl);
        e.Add(t);
        e.Add(v);

        Tick(world, 1);

        Assert.True(t.Position.X > 0, "Позиция должна сдвинуться вправо");
    }

    // ============================================================
    // Изоляция
    // ============================================================

    [Fact]
    public void SystemIgnoresEntitiesWithoutController()
    {
        var world = new World();
        var e = world.Create();
        e.Add(new Transform());
        e.Add(new Velocity { Value = new Vector2(10, 10) });

        Tick(world, 10);

        var v = e.Get<Velocity>()!;
        // Скорость не изменилась — системы не тронули
        Assert.Equal(10f, v.Value.X);
        Assert.Equal(10f, v.Value.Y);
    }

    [Fact]
    public void SystemIgnoresEntitiesWithoutVelocity()
    {
        var world = new World();
        var e = world.Create();
        e.Add(new PlatformerController());
        e.Add(new Transform());

        // Не должно кидать исключение
        Tick(world, 5);

        Assert.True(true, "Не упало без Velocity");
    }
}