using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.Events;
using MyEngine.Rendering;
using MyEngine;

namespace FactoryGame.States;

public sealed class GameContext
{
    public Application App = null!;
    public World World = null!;
    public GameState State = null!;
    public Entity Player = null!;
    public EventBus Events = null!;

    public SystemScheduler UpdateSystems = new();
    public SystemScheduler VariableSystems = new();

    public ParticleSystem Particles = null!;
    public Camera2D Camera = null!;
    public Font Font = null!;

    public Profiler Profiler = null!;
    public WorldInspectorWindow WorldInspector = null!;

    public Action Restart = null!;
    public Action FinishLoading = null!;
}