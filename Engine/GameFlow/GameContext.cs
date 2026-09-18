using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.Events;
using MyEngine.Rendering;

namespace MyEngine.GameFlow;

public class GameContext
{
    public Application App = null!;
    public World World = null!;
    public Camera2D Camera = null!;
    public EventBus Events = null!;
    public ParticleSystem Particles = null!;
    public Font Font = null!;

    // Опционально — игра может положить свои сервисы:
    private readonly Dictionary<Type, object> _services = new();

    public void SetService<T>(T service) where T : class
        => _services[typeof(T)] = service;

    public T? GetService<T>() where T : class
        => _services.GetValueOrDefault(typeof(T)) as T;
}