using System.Numerics;

namespace MyEngine.Ai;

/// <summary>
/// Поиск кратчайшего пути по растровой сетке проходимости (NavGrid).
/// Классический A* с 8 направлениями (4 прямых + 4 диагонали).
///
/// Возвращает список мировых координат (центры клеток) от старта к цели,
/// либо null, если путь не найден.
///
/// Не выделяет память в hot loop — рабочие массивы создаются один раз на вызов.
/// Для карт 64×64 и меньше работает за миллисекунды.
/// </summary>
public static class AStar
{
    /// <summary>
    /// Смещения к соседям: (dx, dy, стоимость шага).
    /// Прямые шаги стоят 1, диагонали — √2 ≈ 1.41421.
    /// Такая стоимость делает диагонали чуть дороже прямых,
    /// чтобы A* предпочитал прямые пути, когда это возможно.
    /// </summary>
    private static readonly (int dx, int dy, float cost)[] Neighbors8 =
    {
        ( 1,  0, 1f), (-1,  0, 1f), ( 0,  1, 1f), ( 0, -1, 1f),
        ( 1,  1, 1.41421f), ( 1, -1, 1.41421f),
        (-1,  1, 1.41421f), (-1, -1, 1.41421f),
    };

    /// <summary>
    /// Ищет путь от start до goal по сетке grid.
    /// </summary>
    /// <param name="grid">Сетка проходимости.</param>
    /// <param name="start">Стартовая точка в мировых координатах.</param>
    /// <param name="goal">Целевая точка в мировых координатах.</param>
    /// <param name="maxIterations">
    /// Защита от бесконечного цикла на плохо построенной сетке.
    /// Если итераций больше — возвращаем null (путь не найден).
    /// 20000 хватает на карты до ~140×140.
    /// </param>
    /// <returns>Список точек пути или null.</returns>
    public static List<Vector2>? FindPath(
        NavGrid grid,
        Vector2 start, Vector2 goal,
        int maxIterations = 20000)
    {
        // Мировые координаты → индексы клеток
        var (sx, sy) = grid.WorldToCell(start);
        var (gx, gy) = grid.WorldToCell(goal);

        // Если старт или цель внутри стены — пути не существует
        if (!grid.IsWalkable(sx, sy) || !grid.IsWalkable(gx, gy))
            return null;

        int w = grid.Width;
        int h = grid.Height;
        int total = w * h;

        // Рабочие массивы. Размер = число клеток, индекс = y * w + x.
        // gScore — стоимость пути от старта до клетки.
        // fScore — gScore + эвристика (оценка полной стоимости через эту клетку).
        // cameFrom — индекс предыдущей клетки в оптимальном пути (для восстановления).
        // closed — клетка уже обработана, больше не переоткрываем.
        // inOpen — клетка уже в очереди open (не добавляем дважды).
        var gScore = new float[total];
        var fScore = new float[total];
        var cameFrom = new int[total];
        var closed = new bool[total];
        var inOpen = new bool[total];

        // Инициализация: все клетки недостижимы, предков нет
        for (int i = 0; i < total; i++)
        {
            gScore[i] = float.MaxValue;
            fScore[i] = float.MaxValue;
            cameFrom[i] = -1;
        }

        int startIdx = sy * w + sx;
        int goalIdx = gy * w + gx;

        gScore[startIdx] = 0;
        fScore[startIdx] = Heuristic(sx, sy, gx, gy);

        // Открытое множество. Для маленьких сеток линейный поиск минимума
        // быстрее, чем полноценная priority queue — меньше аллокаций и overhead.
        // Для сеток 500×500+ стоит заменить на бинарную кучу.
        var open = new List<int> { startIdx };
        inOpen[startIdx] = true;

        int iterations = 0;
        while (open.Count > 0)
        {
            if (++iterations > maxIterations) return null;

            // Выбираем клетку с минимальным F — она наиболее перспективна
            int bestPos = 0;
            float bestF = fScore[open[0]];
            for (int i = 1; i < open.Count; i++)
            {
                if (fScore[open[i]] < bestF)
                {
                    bestF = fScore[open[i]];
                    bestPos = i;
                }
            }

            int current = open[bestPos];
            open.RemoveAt(bestPos);
            inOpen[current] = false;

            // Достигли цели — восстанавливаем путь
            if (current == goalIdx)
                return ReconstructPath(grid, cameFrom, current);

            closed[current] = true;

            // Перебор 8 соседей текущей клетки
            int cx = current % w;
            int cy = current / w;

            foreach (var (dx, dy, cost) in Neighbors8)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                // Границы карты
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (!grid.IsWalkable(nx, ny)) continue;

                // Диагональ разрешена только если ОБЕ смежные клетки (по X и по Y)
                // проходимы. Иначе агент "срежет угол" и пройдёт сквозь стену.
                if (dx != 0 && dy != 0)
                {
                    if (!grid.IsWalkable(cx + dx, cy)) continue;
                    if (!grid.IsWalkable(cx, cy + dy)) continue;
                }

                int nIdx = ny * w + nx;
                if (closed[nIdx]) continue; // уже обработана

                float tentativeG = gScore[current] + cost;

                // Нашли более короткий путь к этому соседу?
                if (tentativeG >= gScore[nIdx]) continue;

                cameFrom[nIdx] = current;
                gScore[nIdx] = tentativeG;
                fScore[nIdx] = tentativeG + Heuristic(nx, ny, gx, gy);

                if (!inOpen[nIdx])
                {
                    open.Add(nIdx);
                    inOpen[nIdx] = true;
                }
            }
        }

        return null; // открытое множество опустело, цель недостижима
    }

    /// <summary>
    /// Оценочная функция (эвристика) — минимальная возможная стоимость
    /// пути между двумя клетками. Используется octile distance:
    /// диагональные шаги "бесплатны" по эвристике, прямые стоят 1.
    ///
    /// Формула: (dx + dy) + (√2 − 2) * min(dx, dy).
    /// Это допустимая (admissible) эвристика — никогда не переоценивает,
    /// что обязательно для корректности A*.
    /// </summary>
    private static float Heuristic(int ax, int ay, int bx, int by)
    {
        int dx = System.Math.Abs(ax - bx);
        int dy = System.Math.Abs(ay - by);
        return (dx + dy) + (1.41421f - 2f) * System.Math.Min(dx, dy);
    }

    /// <summary>
    /// Идёт по цепочке cameFrom от цели к старту, собирает мировые
    /// координаты клеток и разворачивает список — получается путь от старта к цели.
    /// </summary>
    private static List<Vector2> ReconstructPath(NavGrid grid, int[] cameFrom, int current)
    {
        var path = new List<Vector2>();
        while (current != -1)
        {
            int x = current % grid.Width;
            int y = current / grid.Width;
            path.Add(grid.CellToWorld(x, y));
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }
}