using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.WorldEngine;

namespace MyEngine.Ticking;

/// <summary>
/// Планировщик обновлений по частоте.
///
/// Каждый тик:
///   1. Обновляет счётчик тиков.
///   2. Для каждой группы — проверяет, наступил ли её тик.
///   3. Если да — обходит сущности с Tickable и вызывает UpdateEntity.
///
/// Стоимость: одна группа = один обход сущностей. Для 100 000 сущностей
/// с 4 группами — это 4 × 100 000 = 400 000 итераций, но обновление
/// логики только у четверти. Скорость растёт кратно.
/// </summary>
public sealed class TickScheduler
{
    private readonly Dictionary<TickRate, List<TickGroup>> _groupsByRate = new();
    private readonly Dictionary<TickRate, List<Entity>> _entitiesByRate = new();
    private readonly List<TickGroup> _allGroups = new();

    private long _currentTick;
    private readonly List<Entity> _queryBuffer = new();

    /// <summary>Текущий номер тика с момента старта.</summary>
    public long CurrentTick => _currentTick;

    /// <summary>Все зарегистрированные группы.</summary>
    public IReadOnlyList<TickGroup> Groups => _allGroups;

    /// <summary>
    /// Менеджер чанков. Если задан — Tick обходит только сущности
    /// в загруженных чанках (с учётом Tickable.OnlyWhenLoaded).
    /// Если null — обходит всех.
    /// </summary>
    private ChunkManager? _chunkManager;

    /// <summary>Подключить ChunkManager. Вызывать один раз при старте.</summary>
    public void AttachChunks(ChunkManager chunks) => _chunkManager = chunks;

    // ============================================================
    // Регистрация групп
    // ============================================================

    public void RegisterGroup(TickGroup group)
    {
        if (!_groupsByRate.TryGetValue(group.DefaultRate, out var list))
        {
            list = new List<TickGroup>();
            _groupsByRate[group.DefaultRate] = list;
        }
        list.Add(group);
        _allGroups.Add(group);
    }

    // ============================================================
    // Регистрация / снятие сущностей
    // ============================================================

    /// <summary>
    /// Зарегистрировать сущность с Tickable. Вызывай при создании.
    /// Или используй RebuildEntityIndex, если проще пройтись разом.
    /// </summary>
    public void RegisterEntity(Entity entity, Tickable tickable)
    {
        var rate = tickable.Rate;
        if (!_entitiesByRate.TryGetValue(rate, out var list))
        {
            list = new List<Entity>();
            _entitiesByRate[rate] = list;
        }
        list.Add(entity);
    }

    public void UnregisterEntity(Entity entity)
    {
        foreach (var list in _entitiesByRate.Values)
            list.Remove(entity);
    }

    /// <summary>
    /// Перестроить индекс сущностей по частотам. Проходит по всем
    /// сущностям с Tickable и распределяет по группам. Очищает старые.
    ///
    /// Вызывай при загрузке, раз в N кадров, или когда состав сильно изменился.
    /// </summary>
    public void RebuildEntityIndex(World world)
    {
        foreach (var list in _entitiesByRate.Values)
            list.Clear();

        foreach (var e in world.With<Tickable>())
        {
            var t = e.Get<Tickable>()!;
            if (!_entitiesByRate.TryGetValue(t.Rate, out var list))
            {
                list = new List<Entity>();
                _entitiesByRate[t.Rate] = list;
            }
            list.Add(e);
        }
    }

    // ============================================================
    // Обновление
    // ============================================================

    /// <summary>
    /// Вызывать раз в тик (в фиксированном апдейте). Внутри
    /// проверяет, какие группы должны обновиться, и вызывает их.
    /// </summary>
    public void Tick(World world, float dt)
    {
        _currentTick++;

        foreach (var (rate, groups) in _groupsByRate)
        {
            int rateInt = (int)rate;

            if (!_entitiesByRate.TryGetValue(rate, out var entities))
                continue;

            // Какая фаза обрабатывается в этом тике
            int currentPhase = (int)(_currentTick % rateInt);

            foreach (var entity in entities)
            {
                if (!entity.IsAlive) continue;

                var tickable = entity.Get<Tickable>();
                if (tickable == null) continue;

                // Проверка чанка
                if (tickable.OnlyWhenLoaded && _chunkManager != null)
                {
                    var t = entity.Get<Transform>();
                    if (t != null)
                    {
                        var chunk = _chunkManager.GetChunk(t.Position);
                        if (chunk == null || !chunk.Loaded) continue;
                    }
                }

                // Фаза: сработает только если совпадает с текущей
                int entityPhase = tickable.Phase % rateInt;
                if (entityPhase != currentPhase) continue;

                foreach (var group in groups)
                {
                    group.UpdateEntity(entity, world, dt);
                }
            }
        }
    }

    /// <summary>
    /// Полный обход всех сущностей по группам. Вызывается, если
    /// группа хочет работать со всеми Tickable сразу (batch update).
    ///
    /// Пример: FlowSystem обрабатывает все конвейеры разом, не по одному.
    /// </summary>
    public void TickBatch(World world, float dt)
    {
        _currentTick++;

        foreach (var (rate, groups) in _groupsByRate)
        {
            int rateInt = (int)rate;
            if (_currentTick % rateInt != 0) continue;

            if (!_entitiesByRate.TryGetValue(rate, out var entities))
                continue;

            // Заполняем буфер «живых» сущностей этого тика
            _queryBuffer.Clear();
            int currentPhase = (int)(_currentTick % rateInt);

            foreach (var e in entities)
            {
                if (!e.IsAlive) continue;
                var tickable = e.Get<Tickable>();
                if (tickable == null) continue;

                // Чанк
                if (tickable.OnlyWhenLoaded && _chunkManager != null)
                {
                    var t = e.Get<Transform>();
                    if (t != null)
                    {
                        var chunk = _chunkManager.GetChunk(t.Position);
                        if (chunk == null || !chunk.Loaded) continue;
                    }
                }

                // Фаза
                int entityPhase = tickable.Phase % rateInt;
                if (entityPhase != currentPhase) continue;

                _queryBuffer.Add(e);
            }

            // Группы получают полный список разом
            foreach (var group in groups)
            {
                if (group is IBatchTickGroup batch)
                    batch.UpdateBatch(_queryBuffer, world, dt);
                else
                {
                    foreach (var e in _queryBuffer)
                        group.UpdateEntity(e, world, dt);
                }
            }
        }
    }

    public void Clear()
    {
        _groupsByRate.Clear();
        _entitiesByRate.Clear();
        _allGroups.Clear();
        _currentTick = 0;
    }
}

/// <summary>
/// Группа, которая хочет обрабатывать сущности разом, а не по одной.
/// Полезно для flow system (конвейеры) — там нужен общий проход.
/// </summary>
public interface IBatchTickGroup
{
    void UpdateBatch(List<Entity> entities, World world, float dt);
}