using System.Numerics;

namespace MyEngine.Effects;

/// <summary>
/// Настройки эмиттера. Описывают "что и как спавнить".
/// Один объект можно применять сколько угодно раз — он не хранит состояние.
/// </summary>
public sealed class ParticleEmitterSettings
{
    // --- Сколько и как долго ---
    public int Count = 10;
    public float Lifetime = 0.5f;
    public float LifetimeVariance = 0.1f; // ± от Lifetime

    // --- Скорость разлёта ---
    public float SpeedMin = 40f;
    public float SpeedMax = 120f;

    /// <summary>Угол разлёта в радианах. Полный круг = 0..2π.</summary>
    public float AngleMin = 0f;
    public float AngleMax = MathF.PI * 2f;

    // --- Движение ---
    public Vector2 Gravity = Vector2.Zero;
    public float Drag = 0f; // 0 = без сопротивления

    // --- Визуал ---
    public Vector2 SizeStart = new(8, 8);
    public Vector2 SizeEnd = new(2, 2);
    public Vector4 ColorStart = Vector4.One;
    public Vector4 ColorEnd = new(1, 1, 1, 0); // затухание в прозрачность
    public float RotationSpeedMax = 0f;

    // --- Форма эмиттера ---
    /// <summary>Радиус круга спавна. 0 = всё из одной точки.</summary>
    public float SpawnRadius = 0f;

    // --- Слой рендера ---
    /// <summary>Чем больше — тем позже рисуется (поверх других).</summary>
    public int Layer = 0;
}