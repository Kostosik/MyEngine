using MyEngine.Serialization.Binary;
using System.Numerics;

namespace MyEngine.Components;

/// <summary>
/// Трансформ сущности. Позиция, вращение, масштаб.
///
/// Иерархия:
///   - Если Parent == null → Position — источник истины (мировая).
///   - Если Parent != null → LocalPosition относительно родителя,
///     Position вычисляется TransformSystem каждый кадр.
///
/// Игра работает с Position, как раньше. Иерархия — опциональна.
/// </summary>
public sealed class Transform : IBinarySerializable
{
    // === Мировые координаты (используются всей игрой) ===
    public Vector2 Position;
    public float Rotation;
    public Vector2 Scale = Vector2.One;

    // === Иерархия ===
    /// <summary>Родитель. Null — корневой трансформ.</summary>
    public Transform? Parent;

    /// <summary>Дети. Управляется через SetParent/DetachChildren.</summary>
    public List<Transform> Children { get; } = new();

    // === Локальные координаты (используются, только если Parent != null) ===
    public Vector2 LocalPosition;
    public float LocalRotation;
    public Vector2 LocalScale = Vector2.One;

    // ============================================================
    // Привязать к родителю
    // ============================================================

    /// <summary>
    /// Привязать к родителю.
    /// worldPositionStays = true: ребёнок остаётся на месте визуально,
    ///                            LocalPosition пересчитывается.
    /// worldPositionStays = false: LocalPosition остаётся, ребёнок «прыгает».
    /// </summary>
    public void SetParent(Transform? parent, bool worldPositionStays = true)
    {
        if (Parent == parent) return;

        // Отцепиться от старого
        Parent?.Children.Remove(this);

        Parent = parent;
        parent?.Children.Add(this);

        if (parent == null)
        {
            // Отвязались — Position теперь мировая (не меняем)
            // LocalPosition игнорируется
            return;
        }

        if (worldPositionStays)
        {
            // Сохранить мировую позицию → пересчитать локальную
            LocalPosition = Position - parent.Position;
            LocalRotation = Rotation - parent.Rotation;
            LocalScale = parent.Scale == Vector2.Zero
                ? Vector2.One
                : Scale / parent.Scale;
        }
        else
        {
            // Использовать текущий LocalPosition → пересчитать мировую
            Position = parent.Position + LocalPosition;
            Rotation = parent.Rotation + LocalRotation;
            Scale = parent.Scale * LocalScale;
        }
    }

    /// <summary>Отцепить всех детей. Они становятся независимыми.</summary>
    public void DetachChildren()
    {
        foreach (var child in Children)
            child.Parent = null;
        Children.Clear();
    }

    /// <summary>Отцепиться от родителя, остаться на месте (мировая позиция сохраняется).</summary>
    public void DetachFromParent()
    {
        Parent?.Children.Remove(this);
        Parent = null;
        // Position уже актуальна, LocalPosition можно сбросить
        LocalPosition = Position;
        LocalRotation = Rotation;
        LocalScale = Scale;
    }

    // ============================================================
    // Установка позиции в мировых / локальных координатах
    // ============================================================

    /// <summary>Установить мировую позицию. Если есть Parent — пересчитается локальная.</summary>
    public void SetWorldPosition(Vector2 worldPos)
    {
        Position = worldPos;
        if (Parent != null)
            LocalPosition = worldPos - Parent.Position;
    }

    /// <summary>Установить локальную позицию. Если есть Parent — пересчитается мировая.</summary>
    public void SetLocalPosition(Vector2 localPos)
    {
        LocalPosition = localPos;
        if (Parent != null)
            Position = Parent.Position + localPos;
        else
            Position = localPos;
    }

    public void Write(BinaryWriter w)
    {
        w.Write(Position);
        w.Write(Rotation);
        w.Write(Scale);
    }

    public void Read(BinaryReader r)
    {
        Position = r.ReadVector2();
        Rotation = r.ReadSingle();
        Scale = r.ReadVector2();
    }
}