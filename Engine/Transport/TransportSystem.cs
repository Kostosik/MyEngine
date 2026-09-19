using MyEngine.Components;
using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Transport;

/// <summary>
/// Двигает содержимое по слотам всех TransportLine в мире.
/// Передаёт между соседними линиями.
///
/// Универсальная: не знает ни про конвейеры, ни про тайлы, ни про размеры.
/// Соседа ищет по направлению и ConnectionRange компонента.
/// </summary>
public sealed class TransportSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 30;

    public void Update(World world, float dt)
    {
        // 1. Сдвиг внутри линий
        foreach (var e in world.Query().With<TransportLine>())
        {
            var line = e.Get<TransportLine>()!;
            line.MoveTimer += dt;

            while (line.MoveTimer >= line.SecondsPerMove)
            {
                line.MoveTimer -= line.SecondsPerMove;
                ShiftSlots(line);
            }
        }

        // 2. Передача между линиями
        foreach (var e in world.Query().With<TransportLine>().With<Transform>())
        {
            var line = e.Get<TransportLine>()!;
            var t = e.Get<Transform>()!;

            if (line.IsOutputEmpty()) continue;

            var next = FindNextLine(world, t.Position, line.Direction, line.ConnectionRange, e);
            if (next == null) continue;

            // Пробуем положить первый слот в соседа
            int leftover = next.TryAdd(line.Slots[0]);
            if (leftover < line.Slots[0].Amount)
            {
                // Что-то ушло
                int moved = line.Slots[0].Amount - leftover;
                line.Slots[0].Amount -= moved;
                if (line.Slots[0].Amount <= 0)
                    line.Slots[0] = TransportSlot.Empty;
            }
        }
    }

    private static void ShiftSlots(TransportLine line)
    {
        for (int i = line.SlotCount - 1; i > 0; i--)
        {
            if (line.Slots[i - 1].IsEmpty) continue;   // впереди пусто
            if (!line.Slots[i].IsEmpty) continue;      // место занято — стоим

            line.Slots[i] = line.Slots[i - 1];
            line.Slots[i - 1] = TransportSlot.Empty;
        }
    }

    /// <summary>
    /// Найти ближайший TransportLine в направлении.
    /// Работает без понятия о тайлах — просто ищет линию в конусе направления.
    /// </summary>
    private static TransportLine? FindNextLine(
        World world,
        Vector2 fromPos,
        int direction,
        float maxRange,
        Entity self)
    {
        var dirVec = DirectionVec(direction);
        TransportLine? best = null;
        float bestDist = maxRange;

        foreach (var e in world.Query().With<TransportLine>().With<Transform>())
        {
            if (ReferenceEquals(e, self)) continue;

            var t = e.Get<Transform>()!;
            var diff = t.Position - fromPos;

            // Проекция на направление
            float along = Vector2.Dot(diff, dirVec);
            if (along <= 0.5f || along > maxRange) continue;

            // Перпендикулярное смещение — должно быть маленьким
            var perpVec = new Vector2(-dirVec.Y, dirVec.X);
            float perp = MathF.Abs(Vector2.Dot(diff, perpVec));
            if (perp > 4f) continue;

            // Ближайший
            if (along < bestDist)
            {
                bestDist = along;
                best = e.Get<TransportLine>();
            }
        }

        return best;
    }

    /// <summary>Единичный вектор направления. 0=вправо, 1=вниз, 2=вверх, 3=влево.</summary>
    public static Vector2 DirectionVec(int direction) => direction switch
    {
        0 => new Vector2(1, 0),
        1 => new Vector2(0, 1),
        2 => new Vector2(0, -1),
        3 => new Vector2(-1, 0),
        _ => Vector2.Zero
    };
}