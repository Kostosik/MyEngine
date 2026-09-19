using MyEngine;
using MyEngine.Components;
using MyEngine.Ecs;

namespace TestRPGGame;

/// <summary>
/// Прогон игровой логики без окна. Для тестов и балансировки.
/// </summary>
public sealed class HeadlessGameRunner : HeadlessApplication
{
    private World _world = null!;

    protected override void Load()
    {
        _world = new World();

        // Создать сущности
        for (int i = 0; i < 100; i++)
        {
            var e = _world.Create();
            e.Add(new Transform { Position = new System.Numerics.Vector2(i * 10, 0) });
            e.Add(new Velocity());
            e.Add(new Health { Hp = 100, MaxHp = 100 });
        }
    }

    protected override void Update(float dt)
    {
        // Тикнуть системы
        // ... например, движение
    }
}