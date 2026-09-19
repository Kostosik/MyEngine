using MyEngine.Components;
using System.Numerics;
using MyEngine.Ecs;
using TestRPGGame.Components;

namespace TestRPGGame.DebugGame;

public static class GameDiagnostics
{
    public static void LogInteractables(World world)
    {
        Console.WriteLine("--- Interactables ---");
        int count = 0;
        foreach (var e in world.With<Interactable>())
        {
            var i = e.Get<Interactable>()!;
            var t = e.Get<Transform>();
            Console.WriteLine($"  [{i.Id}] '{i.Speaker}' at {t?.Position} r={i.Radius}");
            count++;
        }
        Console.WriteLine($"Total: {count}");
    }

    public static void LogPickups(World world)
    {
        Console.WriteLine("--- Pickups ---");
        int count = 0;
        foreach (var e in world.With<Pickup>())
        {
            var p = e.Get<Pickup>()!;
            var t = e.Get<Transform>();
            Console.WriteLine($"  [{p.Id}] kind={p.Kind} at {t?.Position}");
            count++;
        }
        Console.WriteLine($"Total: {count}");
    }

    public static void LogEnemies(World world)
    {
        Console.WriteLine("--- Enemies ---");
        int count = 0;
        foreach (var e in world.With<EnemyTag>())
        {
            var hp = e.Get<Health>();
            var t = e.Get<Transform>();
            Console.WriteLine($"  HP={hp?.Hp}/{hp?.MaxHp} at {t?.Position}");
            count++;
        }
        Console.WriteLine($"Total: {count}");
    }
}