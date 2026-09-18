using MyEngine.Components;

namespace MyEngine.Physics;

public static class CollisionMatrix
{
    /// <summary>
    /// Должны ли два объекта сталкиваться.
    /// Правило: достаточно, чтобы ХОТЯ БЫ ОДИН из них хотел
    /// столкнуться со слоем второго. Это устраняет ошибки "забыл
    /// прописать с двух сторон".
    /// </summary>
    public static bool ShouldCollide(Collider a, Collider b)
    {
        return (a.CollidesWith & b.Layer) != 0
            || (b.CollidesWith & a.Layer) != 0;
    }
}