namespace MyEngine.Ticking;

/// <summary>
/// Пометить сущность как «обновляется по расписанию».
/// Само обновление — в конкретной системе, зарегистрированной
/// в TickScheduler через TickGroup.
///
/// Пример:
///   var furnace = world.Create();
///   furnace.Add(new Transform { ... });
///   furnace.Add(new Tickable { Rate = TickRate.Every4 });
///   // в системе SmelterSystem.Update сущности с Tickable обновляются раз в 4 тика
/// </summary>
public sealed class Tickable
{
    /// <summary>Частота обновления.</summary>
    public TickRate Rate = TickRate.EveryTick;

    /// <summary>
    /// «Фаза» внутри группы. Задержка перед первым срабатыванием —
    /// 0..Rate-1. Разные фазы распределяют нагрузку по тикам.
    /// </summary>
    public int Phase;

    /// <summary>
    /// Обрабатывать только если чанк сущности загружен.
    /// Для больших миров — не тратить CPU на невидимое.
    /// </summary>
    public bool OnlyWhenLoaded = true;
}