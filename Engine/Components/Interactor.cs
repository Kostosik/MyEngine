using MyEngine.Ecs;

namespace MyEngine.Components;

/// <summary>
/// Маркер: сущность, которая ищет Interactable рядом.
/// Обычно ставится игроку. Может быть и у NPC (например, для поиска
/// объектов, с которыми можно взаимодействовать).
/// </summary>
public sealed class Interactor
{
    /// <summary>Текущая цель — ближайший Interactable в радиусе. Обновляет InteractionSystem.</summary>
    public Entity? CurrentTarget;
}