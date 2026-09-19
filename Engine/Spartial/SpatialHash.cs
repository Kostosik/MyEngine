using MyEngine.Math;
using System.Numerics;

namespace MyEngine.Spartial;

/// <summary>
/// Раскладка AABB-объектов по сетке клеток фиксированного размера.
/// Один объект лежит во всех клетках, которые пересекает его AABB.
/// Не потокобезопасен.
/// </summary>
public sealed class SpatialHash<T> where T : class
{
    private readonly float _cellSize;
    private readonly Dictionary<(int, int), List<T>> _cells = new();
    private readonly HashSet<T> _seen = new();

    public SpatialHash(float cellSize = 128f)
    {
        if (cellSize <= 0) throw new ArgumentException("cellSize must be > 0", nameof(cellSize));
        _cellSize = cellSize;
    }

    public float CellSize => _cellSize;
    public int CellCount => _cells.Count;

    public void Clear()
    {
        foreach (var list in _cells.Values) list.Clear();
        // Оставляем пустые списки — их дешевле переиспользовать, чем пересоздавать
    }

    public void Insert(T item, Aabb box)
    {
        foreach (var key in CellsFor(box))
        {
            if (!_cells.TryGetValue(key, out var list))
            {
                list = new List<T>(4);
                _cells[key] = list;
            }
            list.Add(item);
        }
    }

    /// <summary>
    /// Кладёт в results все объекты, чьи AABB пересекаются с запросом.
    /// Дубликаты (объект в нескольких клетках) отсеиваются.
    /// </summary>
    public void Query(Aabb box, List<T> results)
    {
        results.Clear();
        _seen.Clear();

        foreach (var key in CellsFor(box))
        {
            if (!_cells.TryGetValue(key, out var list)) continue;
            foreach (var item in list)
            {
                if (_seen.Add(item)) results.Add(item);
            }
        }
    }

    private IEnumerable<(int, int)> CellsFor(Aabb box)
    {
        int minX = (int)System.Math.Floor(box.Min.X / _cellSize);
        int maxX = (int)System.Math.Floor(box.Max.X / _cellSize);
        int minY = (int)System.Math.Floor(box.Min.Y / _cellSize);
        int maxY = (int)System.Math.Floor(box.Max.Y / _cellSize);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                yield return (x, y);
    }
}