using MyEngine.Ecs;
using System.Text;

namespace MyEngine.Diagnostics;

public static class WorldInspector
{
    /// <summary>
    /// Снимок: словарь "имя компонента" → количество сущностей с ним.
    /// Плюс общее количество живых сущностей.
    /// </summary>
    public static Dictionary<string, int> CountByComponent(World world)
    {
        var counts = new Dictionary<string, int>();
        int total = 0;

        foreach (var e in world.Entities)
        {
            if (!e.IsAlive) continue;
            total++;

            foreach (var compType in e.ComponentTypes)
            {
                var name = compType.Name;
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }
        }

        counts["__total"] = total;
        return counts;
    }

    /// <summary>
    /// Строка-отчёт — удобно писать в консоль.
    /// </summary>
    public static string Report(World world)
    {
        var counts = CountByComponent(world);
        var sb = new StringBuilder();
        sb.AppendLine("=== World snapshot ===");

        int total = counts.GetValueOrDefault("__total");
        sb.AppendLine($"Total entities: {total}");

        foreach (var kv in counts.OrderBy(kv => kv.Key))
        {
            if (kv.Key == "__total") continue;
            sb.AppendLine($"  {kv.Key,-20} {kv.Value}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Список сущностей, у которых есть компонент типа T.
    /// Возвращает пары (сущность, индекс) — чтобы можно было вывести что-то осмысленное.
    /// </summary>
    public static List<Entity> With<T>(World world) where T : class
    {
        var list = new List<Entity>();
        foreach (var e in world.With<T>()) list.Add(e);
        return list;
    }
}