using System.Numerics;

namespace MyEngine.Effects;

/// <summary>
/// Одна частица. Struct — хранится в массиве без boxing, никакого GC.
/// Все визуальные параметры интерполируются от Start к End по прогрессу жизни.
/// </summary>
public struct Particle
{
    // --- Физика ---
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration; // обычно гравитация (0, +300) или (0, 0)

    // --- Жизнь ---
    public float Life;      // сколько ещё живёт (в секундах)
    public float MaxLife;   // начальная жизнь — нужна для расчёта прогресса

    // --- Визуал ---
    public Vector2 SizeStart;
    public Vector2 SizeEnd;
    public Vector4 ColorStart;
    public Vector4 ColorEnd;
    public float Rotation;
    public float RotationSpeed;

    // --- Служебное ---
    public bool Active;
}