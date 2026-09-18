namespace MyEngine.Ecs;

public interface ISystem
{
    void Update(World world, float dt);

    /// <summary>
    /// Фаза выполнения. По умолчанию Update — обычная логика.
    /// Переопредели, если системе нужен особый порядок.
    /// </summary>
    SystemPhase Phase => SystemPhase.Update;

    /// <summary>
    /// Приоритет внутри фазы. Меньше — раньше. По умолчанию 0.
    /// </summary>
    int Priority => 0;
}