using System.Numerics;

namespace MyEngine.Effects;

/// <summary>
/// Сервис частиц. Хранит пул, обновляет его, спавнит новые частицы
/// по настройкам эмиттера.
///
/// Не является частью ECS — частицы не сущности, а простые POD-структуры
/// в массиве. Это нужно для производительности (тысячи частиц без накладных
/// расходов на компоненты).
/// </summary>
public sealed class ParticleSystem
{
    private readonly ParticlePool _pool;
    private readonly Random _rng = new();

    public ParticleSystem(int capacity = 4096)
    {
        _pool = new ParticlePool(capacity);
    }

    public ParticlePool Pool => _pool;
    public int Capacity => _pool.Capacity;
    public int ActiveCount => _pool.ActiveCount();

    /// <summary>
    /// Обновить все активные частицы.
    /// Вызывать раз в кадр (обычно из fixed update — 60 Гц).
    /// </summary>
    public void Update(float dt)
    {
        _pool.Update(dt);
    }

    /// <summary>
    /// Спавнит группу частиц по настройкам в точке origin.
    /// </summary>
    public void Emit(ParticleEmitterSettings s, Vector2 origin)
    {
        for (int i = 0; i < s.Count; i++)
            EmitOne(s, origin);
    }

    private void EmitOne(ParticleEmitterSettings s, Vector2 origin)
    {
        ref var p = ref _pool.NextFree();

        // Позиция — точка или случайная внутри круга SpawnRadius
        Vector2 offset = Vector2.Zero;
        if (s.SpawnRadius > 0f)
        {
            float angle = RandomFloat(0f, MathF.PI * 2f);
            float r = MathF.Sqrt(RandomFloat(0f, 1f)) * s.SpawnRadius;
            offset = new Vector2(MathF.Cos(angle) * r, MathF.Sin(angle) * r);
        }

        // Направление и скорость
        float dirAngle = RandomFloat(s.AngleMin, s.AngleMax);
        float speed = RandomFloat(s.SpeedMin, s.SpeedMax);
        var velocity = new Vector2(MathF.Cos(dirAngle), MathF.Sin(dirAngle)) * speed;

        // Жизнь
        float life = s.Lifetime;
        if (s.LifetimeVariance > 0f)
            life += RandomFloat(-s.LifetimeVariance, s.LifetimeVariance);
        if (life <= 0.01f) life = 0.01f;

        // Заполняем
        p.Position = origin + offset;
        p.Velocity = velocity;
        p.Acceleration = s.Gravity;
        p.Life = life;
        p.MaxLife = life;
        p.SizeStart = s.SizeStart;
        p.SizeEnd = s.SizeEnd;
        p.ColorStart = s.ColorStart;
        p.ColorEnd = s.ColorEnd;
        p.Rotation = RandomFloat(0f, MathF.PI * 2f);
        p.RotationSpeed = s.RotationSpeedMax > 0f
            ? RandomFloat(-s.RotationSpeedMax, s.RotationSpeedMax)
            : 0f;
        p.Active = true;
    }

    public void Clear() => _pool.Clear();

    private float RandomFloat(float min, float max)
        => min + (float)_rng.NextDouble() * (max - min);
}