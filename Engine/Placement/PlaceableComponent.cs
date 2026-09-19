using System.Numerics;

namespace MyEngine.Placement;

/// <summary>
/// Маркер: сущность можно ставить на сетку.
/// Хранит размер в тайлах и флаг твёрдости.
/// </summary>
public sealed class PlaceableComponent
{
    /// <summary>Имя типа для консоли: "conveyor", "furnace".</summary>
    public string TypeId = "";

    /// <summary>Размер здания в тайлах.</summary>
    public Vector2 SizeInTiles = Vector2.One;

    /// <summary>Блокирует ли клетку для других зданий.</summary>
    public bool IsSolid = true;

    /// <summary>Можно ли поворачивать.</summary>
    public bool CanRotate = true;
}