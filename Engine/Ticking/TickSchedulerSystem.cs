using MyEngine.Ecs;

namespace MyEngine.Ticking;

/// <summary>
/// Система-обёртка. Регистрируется в SystemScheduler и вызывает
/// TickScheduler.Tick каждый фиксированный апдейт.
/// </summary>
public sealed class TickSchedulerSystem : ISystem
{
    private readonly TickScheduler _scheduler;
    private readonly bool _useBatch;

    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 50;   // после большинства систем, до постобработки

    public TickSchedulerSystem(TickScheduler scheduler, bool useBatch = false)
    {
        _scheduler = scheduler;
        _useBatch = useBatch;
    }

    public void Update(World world, float dt)
    {
        if (_useBatch)
            _scheduler.TickBatch(world, dt);
        else
            _scheduler.Tick(world, dt);
    }
}