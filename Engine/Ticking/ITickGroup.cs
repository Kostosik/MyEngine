using MyEngine.Ecs;

namespace MyEngine.Ticking;

/// <summary>
/// Группа обновления. Регистрируется в TickScheduler с частотой.
/// Все системы внутри группы тикают одновременно, только когда
/// пришёл их тик.
///
/// Примеры:
///   - "Machines" — TickRate.EveryTick — конвейеры, inserter'ы
///   - "Furnaces" — TickRate.Every4 — печи, плавильни
///   - "BigMachines" — TickRate.Every16 — сборщики, лаборатории
/// </summary>
public abstract class TickGroup
{
    /// <summary>Имя группы (для отладки и профайлера).</summary>
    public string Name { get; set; } = "";

    /// <summary>Частота группы. Может быть переопределена в компоненте Tickable.</summary>
    public TickRate DefaultRate { get; set; } = TickRate.EveryTick;

    /// <summary>
    /// Обновить одну сущность. Вызывается только когда наступил тик
    /// этой сущности (с учётом её Tickable.Phase).
    /// </summary>
    public abstract void UpdateEntity(Entity entity, World world, float dt);
}

/// <summary>
/// Группа через лямбду. Удобно для простых случаев.
/// </summary>
public sealed class LambdaTickGroup : TickGroup
{
    private readonly Action<Entity, World, float> _update;

    public LambdaTickGroup(string name, TickRate rate, Action<Entity, World, float> update)
    {
        Name = name;
        DefaultRate = rate;
        _update = update;
    }

    public override void UpdateEntity(Entity entity, World world, float dt)
        => _update(entity, world, dt);
}