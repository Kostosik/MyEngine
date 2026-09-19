using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Events;
using MyEngine.Math;
using MyEngine.Physics;
using MyEngine.Spartial;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Обрабатывает триггерные зоны: коллайдеры с IsTrigger = true.
/// Публикует TriggerEnterEvent / TriggerExitEvent через EventBus.
///
/// Триггеры не разрешают столкновения — их обрабатывает CollisionSystem
/// с явным пропуском.
/// </summary>
public sealed class TriggerSystem : ISystem
{
    private readonly EventBus _events;
    private readonly SpatialHash<Entity> _triggerHash = new(cellSize: 128f);
    private readonly List<Entity> _queryBuffer = new(16);
    private Dictionary<Entity, HashSet<Entity>> _current = new();
    private Dictionary<Entity, HashSet<Entity>> _previous = new();

    public SystemPhase Phase => SystemPhase.PostUpdate;
    public int Priority => 30;

    public TriggerSystem(EventBus events)
    {
        _events = events;
    }

    public void Update(World world, float dt)
    {
        // 1. Собрать активные триггеры в хеш
        _triggerHash.Clear();
        foreach (var e in world.With<Collider>())
        {
            var c = e.Get<Collider>()!;
            if (!c.IsTrigger) continue;
            var t = e.Get<Transform>()!;
            _triggerHash.Insert(e, Aabb.FromCenterSize(t.Position, c.Size));
        }

        // 2. Свап: current ← previous, чистим current для заполнения
        (_previous, _current) = (_current, _previous);
        foreach (var set in _current.Values) set.Clear();

        // 3. Для каждой не-триггерной сущности — проверяем триггеры рядом
        foreach (var e in world.With<Collider>())
        {
            var c = e.Get<Collider>()!;
            if (c.IsTrigger) continue;
            if (!e.IsAlive) continue;

            var t = e.Get<Transform>()!;
            var selfBox = Aabb.FromCenterSize(t.Position, c.Size);

            _triggerHash.Query(selfBox, _queryBuffer);

            foreach (var trigger in _queryBuffer)
            {
                var tc = trigger.Get<Collider>()!;
                if ((c.CollidesWith & tc.Layer) == 0) continue;

                var tt = trigger.Get<Transform>()!;
                var triggerBox = Aabb.FromCenterSize(tt.Position, tc.Size);
                if (!selfBox.Intersects(triggerBox)) continue;

                if (!_current.TryGetValue(trigger, out var set))
                {
                    set = new HashSet<Entity>();
                    _current[trigger] = set;
                }
                set.Add(e);
            }
        }

        // 4. Сравниваем с previous — публикуем Enter/Exit
        foreach (var kv in _current)
        {
            var trigger = kv.Key;
            var curSet = kv.Value;
            var prevSet = _previous.GetValueOrDefault(trigger);

            if (prevSet != null)
            {
                foreach (var e in prevSet)
                {
                    if (!curSet.Contains(e))
                        _events.Publish(new TriggerExitEvent { Trigger = trigger, Other = e });
                }
            }

            foreach (var e in curSet)
            {
                if (prevSet == null || !prevSet.Contains(e))
                    _events.Publish(new TriggerEnterEvent { Trigger = trigger, Other = e });
            }
        }

        // 5. Триггеры, из которых все вышли
        foreach (var kv in _previous)
        {
            if (_current.ContainsKey(kv.Key)) continue;
            foreach (var e in kv.Value)
                _events.Publish(new TriggerExitEvent { Trigger = kv.Key, Other = e });
        }

        // 6. Убрать мёртвые триггеры из истории
        var dead = new List<Entity>();
        foreach (var kv in _current)
            if (!kv.Key.IsAlive) dead.Add(kv.Key);
        foreach (var e in dead) _current.Remove(e);

        dead.Clear();
        foreach (var kv in _previous)
            if (!kv.Key.IsAlive) dead.Add(kv.Key);
        foreach (var e in dead) _previous.Remove(e);
    }
}