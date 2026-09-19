using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.GameFlow;
using MyEngine.UI;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.States;

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
    public DamagePopupSystem Popups = null!;

    // Системы (для игры — не для движка)
    public SystemScheduler UpdateSystems = null!;
    public SystemScheduler VariableSystems = null!;



    // UI
    public LevelUpMenu Menu = null!;
    public GameOverScreen GameOver = null!;
    public DialogueBox Dialogue = null!;

    // Прочее
    public Profiler Profiler = null!;
    public WorldInspectorWindow WorldInspector = null!;

    public Action Restart = null!;
    public Action FinishLoading = null!;
}