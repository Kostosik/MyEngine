using System.Numerics;

namespace MyEngine.Effects;

/// <summary>
/// Пул частиц фиксированной ёмкости.
/// Использует кольцевой буфер — при переполнении новые частицы
/// перезаписывают самые старые. Никаких аллокаций в рантайме.
/// </summary>
public sealed class ParticlePool
{
    private readonly Particle[] _particles;
    private int _nextIndex;

    public ParticlePool(int capacity)
    {
        if (capacity <= 0) throw new ArgumentException("Capacity must be > 0");
        _particles = new Particle[capacity];
    }

    public int Capacity => _particles.Length;

    /// <summary>Массив частиц для чтения (например, рендером).</summary>
    public ReadOnlySpan<Particle> Particles => _particles;

    /// <summary>
    /// Вернуть ссылку на следующую свободную (или самую старую) частицу.
    /// Вызывающий сам заполняет её поля. Не помечает Active = true —
    /// это делает эмиттер после заполнения.
    /// </summary>
    public ref Particle NextFree()
    {
        ref var p = ref _particles[_nextIndex];
        _nextIndex = (_nextIndex + 1) % _particles.Length;
        return ref p;
    }

    public void Update(float dt)
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            ref var p = ref _particles[i];
            if (!p.Active) continue;

            p.Life -= dt;
            if (p.Life <= 0f)
            {
                p.Active = false;
                continue;
            }

            p.Velocity += p.Acceleration * dt;
            p.Position += p.Velocity * dt;
            p.Rotation += p.RotationSpeed * dt;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _particles.Length; i++)
            _particles[i].Active = false;
    }

    public int ActiveCount()
    {
        int c = 0;
        for (int i = 0; i < _particles.Length; i++)
            if (_particles[i].Active) c++;
        return c;
    }
}