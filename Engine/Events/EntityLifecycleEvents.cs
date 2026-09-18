using MyEngine.Ecs;

namespace MyEngine.Events;

/// <summary>
/// Сущность создана. Публикуется World.Create() через EventBus,
/// если он подключён через World.AttachEvents().
/// </summary>
public sealed class EntityCreatedEvent
{
    public Entity Entity = null!;
}

/// <summary>
/// Сущность уничтожена. Публикуется World.Destroy().
/// ВАЖНО: сущность уже помечена IsAlive = false, но её компоненты
/// ещё доступны для чтения в обработчике.
/// </summary>
public sealed class EntityDestroyedEvent
{
    public Entity Entity = null!;
}