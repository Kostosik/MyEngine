using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Ticking;
using MyEngine.WorldEngine;

namespace MyEngine.Tests;

public class TickSchedulerTests
{
    private class CountingGroup : TickGroup
    {
        public int UpdateCount;
        public List<int> UpdatedEntityIds = new();

        public CountingGroup(string name, TickRate rate)
        {
            Name = name;
            DefaultRate = rate;
        }

        public override void UpdateEntity(Entity entity, World world, float dt)
        {
            UpdateCount++;
            UpdatedEntityIds.Add(entity.Id);
        }
    }

    private static (World world, Entity entity) MakeTickableWorld(
        TickRate rate = TickRate.EveryTick, int phase = 0)
    {
        var world = new World();
        var e = world.Create();
        e.Add(new Transform());
        e.Add(new Tickable { Rate = rate, Phase = phase });
        return (world, e);
    }

    // ============================================================
    // EveryTick — каждый тик
    // ============================================================

    [Fact]
    public void EveryTick_UpdatesEveryTick()
    {
        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var (world, _) = MakeTickableWorld(TickRate.EveryTick);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 10; i++) sched.Tick(world, 1f / 60f);

        Assert.Equal(10, group.UpdateCount);
    }

    // ============================================================
    // Every4 — раз в 4 тика
    // ============================================================

    [Fact]
    public void Every4_UpdatesOnceIn4Ticks()
    {
        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.Every4);
        sched.RegisterGroup(group);

        var (world, _) = MakeTickableWorld(TickRate.Every4);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 12; i++) sched.Tick(world, 1f / 60f);

        // 12 тиков / 4 = 3 обновления
        Assert.Equal(3, group.UpdateCount);
    }

    [Fact]
    public void Every16_UpdatesOnceIn16Ticks()
    {
        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.Every16);
        sched.RegisterGroup(group);

        var (world, _) = MakeTickableWorld(TickRate.Every16);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 64; i++) sched.Tick(world, 1f / 60f);

        Assert.Equal(4, group.UpdateCount);
    }

    // ============================================================
    // Фазы
    // ============================================================

    [Fact]
    public void Phase_ShiftsTiming()
    {
        // Две сущности с одинаковой частотой, разной фазой.
        // За 8 тиков каждая должна сработать 2 раза (при rate 4).
        var world = new World();

        var e1 = world.Create();
        e1.Add(new Tickable { Rate = TickRate.Every4, Phase = 0 });

        var e2 = world.Create();
        e2.Add(new Tickable { Rate = TickRate.Every4, Phase = 2 });

        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.Every4);
        sched.RegisterGroup(group);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 8; i++) sched.Tick(world, 1f / 60f);

        // Каждая по 2 раза — итого 4
        Assert.Equal(4, group.UpdateCount);
    }

    // ============================================================
    // Несколько сущностей одной частоты
    // ============================================================

    [Fact]
    public void MultipleEntities_SameRate_AllUpdate()
    {
        var world = new World();
        for (int i = 0; i < 5; i++)
        {
            var e = world.Create();
            e.Add(new Tickable { Rate = TickRate.EveryTick });
        }

        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 10; i++) sched.Tick(world, 1f / 60f);

        Assert.Equal(50, group.UpdateCount);   // 5 × 10
    }

    // ============================================================
    // Группы разных частот
    // ============================================================

    [Fact]
    public void DifferentRates_UpdateIndependently()
    {
        var world = new World();

        var eEveryTick = world.Create();
        eEveryTick.Add(new Tickable { Rate = TickRate.EveryTick });

        var eEvery4 = world.Create();
        eEvery4.Add(new Tickable { Rate = TickRate.Every4 });

        var sched = new TickScheduler();
        var groupEveryTick = new CountingGroup("tick", TickRate.EveryTick);
        var groupEvery4 = new CountingGroup("t4", TickRate.Every4);
        sched.RegisterGroup(groupEveryTick);
        sched.RegisterGroup(groupEvery4);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 12; i++) sched.Tick(world, 1f / 60f);

        Assert.Equal(12, groupEveryTick.UpdateCount);  // 12 тиков
        Assert.Equal(3, groupEvery4.UpdateCount);      // 12 / 4
    }

    // ============================================================
    // Мёртвые сущности игнорируются
    // ============================================================

    [Fact]
    public void DeadEntities_NotUpdated()
    {
        var (world, e) = MakeTickableWorld(TickRate.EveryTick);

        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);
        sched.RebuildEntityIndex(world);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);

        // Убить
        world.Destroy(e);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);  // не увеличилось
    }

    // ============================================================
    // Batch-режим
    // ============================================================

    private class BatchCountingGroup : TickGroup, IBatchTickGroup
    {
        public int BatchCalls;
        public int TotalEntities;

        public BatchCountingGroup(string name, TickRate rate)
        {
            Name = name;
            DefaultRate = rate;
        }

        public override void UpdateEntity(Entity entity, World world, float dt)
        {
            // не используется в batch
        }

        public void UpdateBatch(List<Entity> entities, World world, float dt)
        {
            BatchCalls++;
            TotalEntities += entities.Count;
        }
    }

    [Fact]
    public void BatchMode_CalledOncePerTick()
    {
        var world = new World();
        for (int i = 0; i < 10; i++)
        {
            var e = world.Create();
            e.Add(new Tickable { Rate = TickRate.EveryTick });
        }

        var sched = new TickScheduler();
        var batch = new BatchCountingGroup("batch", TickRate.EveryTick);
        sched.RegisterGroup(batch);
        sched.RebuildEntityIndex(world);

        for (int i = 0; i < 5; i++) sched.TickBatch(world, 1f / 60f);

        Assert.Equal(5, batch.BatchCalls);        // 5 раз вызван
        Assert.Equal(50, batch.TotalEntities);    // 10 × 5 сущностей обработано
    }

    // ============================================================
    // Регистрация / отмена
    // ============================================================

    [Fact]
    public void RegisterEntity_Immediately()
    {
        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        var t = new Tickable { Rate = TickRate.EveryTick };
        e.Add(t);
        sched.RegisterEntity(e, t);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);
    }

    [Fact]
    public void UnregisterEntity_Removes()
    {
        var sched = new TickScheduler();
        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        var t = new Tickable { Rate = TickRate.EveryTick };
        e.Add(t);
        sched.RegisterEntity(e, t);
        sched.Tick(world, 1f / 60f);

        sched.UnregisterEntity(e);
        sched.Tick(world, 1f / 60f);

        Assert.Equal(1, group.UpdateCount);
    }

    [Fact]
    public void OnlyWhenLoaded_SkipsUnloadedChunks()
    {
        var sched = new TickScheduler();
        var chunks = new ChunkManager(chunkSize: 32, tileSize: 16f);
        sched.AttachChunks(chunks);

        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        e.Add(new Transform { Position = System.Numerics.Vector2.Zero });
        e.Add(new Tickable { Rate = TickRate.EveryTick, OnlyWhenLoaded = true });
        sched.RebuildEntityIndex(world);

        // Чанк не загружен — сущность не должна обновляться
        sched.Tick(world, 1f / 60f);
        Assert.Equal(0, group.UpdateCount);

        // Загружаем чанк вокруг нуля
        chunks.LoadAround(System.Numerics.Vector2.Zero, radiusChunks: 1);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);
    }

    [Fact]
    public void OnlyWhenLoaded_FalseIgnoresChunks()
    {
        var sched = new TickScheduler();
        var chunks = new ChunkManager(chunkSize: 32, tileSize: 16f);
        sched.AttachChunks(chunks);

        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        e.Add(new Transform { Position = System.Numerics.Vector2.Zero });
        e.Add(new Tickable { Rate = TickRate.EveryTick, OnlyWhenLoaded = false });
        sched.RebuildEntityIndex(world);

        // Чанк не загружен, но OnlyWhenLoaded = false → обновляется
        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);
    }

    [Fact]
    public void NoChunkManager_UpdatesAll()
    {
        var sched = new TickScheduler();
        // Не подключаем ChunkManager

        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        e.Add(new Transform());
        e.Add(new Tickable { Rate = TickRate.EveryTick, OnlyWhenLoaded = true });
        sched.RebuildEntityIndex(world);

        sched.Tick(world, 1f / 60f);

        // ChunkManager не задан — обновляем всех
        Assert.Equal(1, group.UpdateCount);
    }

    [Fact]
    public void UnloadChunk_StopsUpdating()
    {
        var sched = new TickScheduler();
        var chunks = new ChunkManager(chunkSize: 32, tileSize: 16f);
        sched.AttachChunks(chunks);

        var group = new CountingGroup("test", TickRate.EveryTick);
        sched.RegisterGroup(group);

        var world = new World();
        var e = world.Create();
        e.Add(new Transform { Position = System.Numerics.Vector2.Zero });
        e.Add(new Tickable { Rate = TickRate.EveryTick, OnlyWhenLoaded = true });
        sched.RebuildEntityIndex(world);

        // Загружаем чанк
        chunks.LoadAround(System.Numerics.Vector2.Zero, 1);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount);

        // Выгружаем все
        chunks.UnloadFar(new System.Numerics.Vector2(10000, 10000), 0);

        sched.Tick(world, 1f / 60f);
        Assert.Equal(1, group.UpdateCount); // не увеличилось
    }
}