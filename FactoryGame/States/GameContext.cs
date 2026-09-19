using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.UI;

namespace FactoryGame.States;

/// <summary>
/// Игровой контекст. Наследник движкового — общие поля (App, World,
/// Camera, Events, Particles, Font) в базовом классе, здесь только
/// специфика LighthouseKeeper.
/// </summary>
public sealed class GameContext : MyEngine.GameFlow.GameContext
{
    // Игровое состояние
    public GameState State = null!;
    public Entity Player = null!;

    // Системы (для игры — не для движка)
    public SystemScheduler UpdateSystems = null!;
    public SystemScheduler VariableSystems = null!;



    // UI


    // Прочее
    public Profiler Profiler = null!;
    public WorldInspectorWindow WorldInspector = null!;

    public Action Restart = null!;
    public Action FinishLoading = null!;
}