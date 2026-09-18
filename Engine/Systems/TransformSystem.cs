using MyEngine.Components;
using MyEngine.Ecs;

namespace MyEngine.Systems;

/// <summary>
/// Обновляет мировые позиции всех трансформов с родителем.
///
/// Работает в PreUpdate — до всех остальных систем. Значит, движение,
/// рендер, коллизии — все видят уже актуальный Position.
///
/// Трансформы без родителя не трогает. Игра работает как раньше.
/// </summary>
public sealed class TransformSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.PreUpdate;
    public int Priority => -100;   // раньше всех в PreUpdate

    public void Update(World world, float dt)
    {
        // Обходим только корневые трансформы и рекурсивно их детей.
        // Порядок Entity в World неважен — DFS сам разберётся.
        foreach (var e in world.With<Transform>())
        {
            var t = e.Get<Transform>()!;
            if (t.Parent == null)
                Propagate(t);
        }
    }

    private static void Propagate(Transform parent)
    {
        foreach (var child in parent.Children)
        {
            child.Position = parent.Position + child.LocalPosition;
            child.Rotation = parent.Rotation + child.LocalRotation;
            child.Scale = parent.Scale * child.LocalScale;

            // Рекурсия вниз по дереву
            Propagate(child);
        }
    }
}