namespace MyEngine.Physics;

/// <summary>
/// Битовые флаги для категорий объектов.
/// До 32 слоёв — обычно хватает с запасом.
///
/// Пример:
///   Collider.Layer = Layer.Player;
///   Collider.CollidesWith = Layer.Wall | Layer.Enemy;
/// </summary>
[Flags]
public enum Layer : uint
{
    None = 0,
    Player = 1u << 0,
    Enemy = 1u << 1,
    Wall = 1u << 2,
    Pickup = 1u << 3,
    Npc = 1u << 4,
    Trigger = 1u << 5,
    Projectile = 1u << 6,
}