using MyEngine.Ecs;

namespace MyEngine.Ai;

/// <summary>
/// Контракт поведения: что-то, что каждый кадр решает, что делать сущности.
/// Реализации: StateMachine, BehaviorTree, скрипт, что угодно.
/// </summary>
public interface IBehavior
{
    void Tick(Entity self, World world, float dt);
}