using MyEngine.Ecs;

namespace MyEngine.Animation;

/// <summary>
/// Событие анимации. Публикуется через EventBus, когда анимация
/// доходит до кадра с заданным тегом.
///
/// Пример: на 3-м кадре атаки событие "hit" — CombatSystem слушает
/// и наносит урон в этот момент.
/// </summary>
public sealed class AnimationEvent
{
    public Entity Entity = null!;
    public string AnimationName = "";
    public string Tag = "";
    public int Frame;
}