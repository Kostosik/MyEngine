using MyEngine.Ecs;

namespace MyEngine.Events;

/// <summary>
/// Кто-то запросил взаимодействие с Interactable. Публикует InteractionSystem.
/// </summary>
public sealed class InteractionRequestedEvent
{
    public Entity Interactor = null!;
    public Entity Target = null!;
}