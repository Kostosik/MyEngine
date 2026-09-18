using MyEngine.Components;
using MyEngine.Ecs;

namespace MyEngine.Systems;

/// <summary>
/// Превращает TopDownController.InputDirection в Velocity и двигает
/// Transform. Работает в переменном апдейте — реагирует в том же кадре,
/// в котором игрок нажал клавишу.
/// </summary>
public sealed class TopDownControllerSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 10;
    public void Update(World world, float dt)
    {

        foreach (var e in world.Query().With<TopDownController>().With<Transform>().With<Velocity>())
        {
            var ctrl = e.Get<TopDownController>()!;
            var t = e.Get<Transform>();
            var v = e.Get<Velocity>();

            var dir = ctrl.InputDirection;
            if (dir.LengthSquared() > 1f)
                dir = System.Numerics.Vector2.Normalize(dir);

            v.Value = dir * ctrl.MaxSpeed * ctrl.SpeedMultiplier;

            // Двигаем сразу — рендер увидит результат в этом же кадре
            t.Position += v.Value * dt;
        }
    }
}