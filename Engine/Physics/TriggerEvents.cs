using MyEngine.Ecs;

namespace MyEngine.Physics;

/// <summary>Сущность вошла в триггер.</summary>
public sealed class TriggerEnterEvent
{
    public Entity Trigger = null!;
    public Entity Other = null!;
}

/// <summary>Сущность вышла из триггера.</summary>
public sealed class TriggerExitEvent
{
    public Entity Trigger = null!;
    public Entity Other = null!;
}