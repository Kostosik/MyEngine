using MyEngine.Components;
using MyEngine.Ecs;

namespace MyEngine.Systems;

/// <summary>
/// Обновляет пульсацию у PointLight. Просто тикает таймер.
/// Само значение используется в рендере через Sin(Time).
/// </summary>
public sealed class LightPulseSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 50;

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<PointLight>())
        {
            var l = e.Get<PointLight>()!;
            if (l.Pulsate) l.Time += dt;
        }
    }
}