using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Threading;

namespace MyEngine.Systems;

/// <summary>
/// Двигает все сущности с Velocity, кроме тех, кем управляют другие системы:
/// ManualMovement (скрипт), TopDownController, PlatformerController.
/// Позиция обновляется по фиксированному шагу (60 Гц).
/// </summary>
public sealed class MovementSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 10;

    private readonly JobSystem _jobs;

    public MovementSystem(JobSystem jobs) => _jobs = jobs;

    /// <summary>
    /// Есть ли у сущности другой контроллер движения. Если да — MovementSystem
    /// её не трогает, иначе будет двойное движение.
    /// </summary>
    private static bool HasExternalController(Entity e)
        => e.Has<ManualMovement>()
        || e.Has<TopDownController>()
        || e.Has<PlatformerController>();

    public void Update(World world, float dt)
    {
        // Первый проход — сколько сущностей надо двигать
        int count = 0;
        foreach (var e in world.With<Velocity>())
        {
            if (HasExternalController(e)) continue;
            count++;
        }
        if (count == 0) return;

        // Второй проход — собираем массив. Фильтр ТОЧНО такой же,
        // иначе получим IndexOutOfRangeException.
        var entities = new Entity[count];
        int i = 0;
        foreach (var e in world.With<Velocity>())
        {
            if (HasExternalController(e)) continue;
            entities[i++] = e;
        }

        _jobs.ParallelFor("Movement", 0, count, idx =>
        {
            var e = entities[idx];
            var t = e.Get<Transform>()!;
            var v = e.Get<Velocity>()!;
            t.Position += v.Value * dt;
        });
    }
}