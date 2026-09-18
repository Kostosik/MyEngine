using MyEngine.Math;
using System.Numerics;

namespace MyEngine.Ai;

/// <summary>
/// Растровая сетка проходимости для pathfinding.
/// Мир делится на квадратные клетки фиксированного размера (CellSize).
/// Каждая клетка помечена как проходимая или стена.
///
/// Сетка строится один раз при загрузке уровня и не меняется в рантайме
/// (если уровень не перестраивается динамически). Не потокобезопасна.
///
/// Пространственные координаты:
///   - индекс клетки: (x, y), x — столбец, y — строка
///   - линейный индекс в массиве: y * Width + x
///   - мировые координаты: Origin + (x + 0.5) * CellSize — центр клетки
/// </summary>
public sealed class NavGrid
{
    /// <summary>
    /// Одномерный массив флагов проходимости.
    /// true — можно пройти, false — стена.
    /// Индекс: y * Width + x.
    /// </summary>
    private readonly bool[] _walkable;

    /// <summary>Ширина сетки в клетках.</summary>
    public int Width { get; }

    /// <summary>Высота сетки в клетках.</summary>
    public int Height { get; }

    /// <summary>Размер одной клетки в мировых единицах (пикселях).</summary>
    public float CellSize { get; }

    /// <summary>
    /// Мировая координата левого верхнего угла клетки (0, 0).
    /// Позволяет разместить сетку не с (0, 0), а с любой точки карты.
    /// </summary>
    public Vector2 Origin { get; }

    public NavGrid(int width, int height, float cellSize, Vector2 origin)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        Origin = origin;

        // По умолчанию весь мир проходим — стены "вырезаются" через BlockAabb.
        _walkable = new bool[width * height];
        for (int i = 0; i < _walkable.Length; i++) _walkable[i] = true;
    }

    /// <summary>
    /// Проходима ли клетка (x, y).
    /// Клетки за пределами сетки считаются непроходимыми — это защищает
    /// от выхода агентов за карту без дополнительных проверок границ.
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false;
        return _walkable[y * Width + x];
    }

    /// <summary>
    /// Установить флаг проходимости клетки.
    /// Молча игнорирует координаты за пределами сетки — удобно при массовой
    /// блокировке AABB, который может частично выходить за карту.
    /// </summary>
    public void SetWalkable(int x, int y, bool walkable)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return;
        _walkable[y * Width + x] = walkable;
    }

    /// <summary>
    /// Отметить все клетки, пересекающие AABB, как непроходимые.
    /// Используется для превращения стен (статичных коллайдеров) в препятствия
    /// на сетке при построении NavGrid.
    ///
    /// Клетка считается пересечённой, если хотя бы часть её площади попадает
    /// внутрь AABB. Это даёт чуть "толще" препятствия, чем реальная стена —
    /// агент не будет тереться об углы.
    /// </summary>
    public void BlockAabb(Aabb box)
    {
        // Округляем диапазон координат до целых клеток.
        // Floor нужен для корректной работы с отрицательными координатами.
        int minX = (int)MathF.Floor((box.Min.X - Origin.X) / CellSize);
        int maxX = (int)MathF.Floor((box.Max.X - Origin.X) / CellSize);
        int minY = (int)MathF.Floor((box.Min.Y - Origin.Y) / CellSize);
        int maxY = (int)MathF.Floor((box.Max.Y - Origin.Y) / CellSize);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                SetWalkable(x, y, false);
    }

    /// <summary>
    /// Индекс клетки → центр этой клетки в мировых координатах.
    /// +0.5f сдвигает точку из левого верхнего угла в центр клетки —
    /// агент идёт по серединкам клеток, а не по их границам.
    /// </summary>
    public Vector2 CellToWorld(int x, int y)
        => Origin + new Vector2((x + 0.5f) * CellSize, (y + 0.5f) * CellSize);

    /// <summary>
    /// Мировые координаты → индекс клетки.
    /// Floor — потому что координата внутри клетки должна давать её индекс,
    /// а не следующей. Деление на CellSize переводит пиксели в "клеточные единицы".
    /// </summary>
    public (int x, int y) WorldToCell(Vector2 world)
    {
        int x = (int)MathF.Floor((world.X - Origin.X) / CellSize);
        int y = (int)MathF.Floor((world.Y - Origin.Y) / CellSize);
        return (x, y);
    }
}