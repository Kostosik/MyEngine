using MyEngine.Effects;
using System.Numerics;

namespace MyEngine.Game.Effects;

/// <summary>
/// Предустановленные эмиттеры для типовых игровых эффектов.
/// Используй как: particles.Emit(ParticlePresets.HitSpark, position);
/// </summary>
public static class ParticlePresets
{
    /// <summary>Искры при попадании по врагу. Жёлто-оранжевые, разлетаются в стороны.</summary>
    public static readonly ParticleEmitterSettings HitSpark = new()
    {
        Count = 12,
        Lifetime = 0.35f,
        LifetimeVariance = 0.1f,
        SpeedMin = 80f,
        SpeedMax = 200f,
        AngleMin = 0f,
        AngleMax = MathF.PI * 2f,
        Gravity = new Vector2(0, 300f),
        SizeStart = new Vector2(6, 6),
        SizeEnd = new Vector2(2, 2),
        ColorStart = new Vector4(1f, 0.9f, 0.3f, 1f),
        ColorEnd = new Vector4(1f, 0.3f, 0.1f, 0f),
        RotationSpeedMax = 8f,
        SpawnRadius = 6f
    };

    /// <summary>Дым при смерти врага. Серо-зелёный, медленно поднимается.</summary>
    public static readonly ParticleEmitterSettings DeathPuff = new()
    {
        Count = 20,
        Lifetime = 0.8f,
        LifetimeVariance = 0.2f,
        SpeedMin = 30f,
        SpeedMax = 100f,
        Gravity = new Vector2(0, -50f),
        SizeStart = new Vector2(12, 12),
        SizeEnd = new Vector2(20, 20),
        ColorStart = new Vector4(0.6f, 0.8f, 0.4f, 0.9f),
        ColorEnd = new Vector4(0.3f, 0.4f, 0.3f, 0f),
        SpawnRadius = 14f
    };

    /// <summary>Вспышка при подборе искры. Жёлтая, расширяется и гаснет.</summary>
    public static readonly ParticleEmitterSettings SparkCollect = new()
    {
        Count = 24,
        Lifetime = 0.5f,
        SpeedMin = 100f,
        SpeedMax = 250f,
        Gravity = Vector2.Zero,
        SizeStart = new Vector2(8, 8),
        SizeEnd = new Vector2(0, 0),
        ColorStart = new Vector4(1f, 1f, 0.5f, 1f),
        ColorEnd = new Vector4(1f, 0.8f, 0.2f, 0f),
        RotationSpeedMax = 4f,
        SpawnRadius = 8f
    };

    /// <summary>Красные искры при уроне игроку.</summary>
    public static readonly ParticleEmitterSettings PlayerHurt = new()
    {
        Count = 16,
        Lifetime = 0.4f,
        SpeedMin = 100f,
        SpeedMax = 220f,
        Gravity = new Vector2(0, 200f),
        SizeStart = new Vector2(6, 6),
        SizeEnd = new Vector2(2, 2),
        ColorStart = new Vector4(1f, 0.3f, 0.3f, 1f),
        ColorEnd = new Vector4(0.6f, 0.1f, 0.1f, 0f),
        SpawnRadius = 8f
    };
}