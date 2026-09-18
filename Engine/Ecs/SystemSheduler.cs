using MyEngine.Diagnostics;

namespace MyEngine.Ecs;

/// <summary>
/// Хранит системы и тикает их в правильном порядке:
/// по фазе (PreUpdate → Update → PostUpdate → LateUpdate),
/// внутри фазы — по приоритету.
///
/// Сортируй один раз после добавления систем.
/// </summary>
public sealed class SystemScheduler
{
    private readonly List<ISystem> _systems = new();
    private bool _sorted;

    public IReadOnlyList<ISystem> Systems => _systems;

    public void Add(ISystem system)
    {
        _systems.Add(system);
        _sorted = false;
    }

    public void Remove(ISystem system)
    {
        _systems.Remove(system);
        _sorted = false;
    }

    public void Clear()
    {
        _systems.Clear();
        _sorted = false;
    }

    public void Update(World world, float dt, Func<ISystem, IDisposable>? scope = null)
    {
        EnsureSorted();
        foreach (var system in _systems)
        {
            if (scope != null)
            {
                using (scope(system))
                    system.Update(world, dt);
            }
            else
            {
                system.Update(world, dt);
            }
        }
    }

    private void EnsureSorted()
    {
        if (_sorted) return;

        _systems.Sort((a, b) =>
        {
            int cmp = a.Phase.CompareTo(b.Phase);
            if (cmp != 0) return cmp;
            return a.Priority.CompareTo(b.Priority);
        });

        _sorted = true;
    }
}