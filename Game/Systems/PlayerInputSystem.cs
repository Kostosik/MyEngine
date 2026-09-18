using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.InputEngine;
using System.Numerics;

namespace MyEngine.Game.Systems;

/// <summary>
/// Читает ввод игрока и записывает вектор движения в TopDownController.
/// Не двигает и не устанавливает Velocity — это делает TopDownControllerSystem.
/// </summary>
public sealed class PlayerInputSystem : ISystem
{
    private readonly Application _app;
    private readonly Entity _player;
    private readonly GameState _state;

    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 0;

    public PlayerInputSystem(Application app, Entity player, GameState state)
    {
        _app = app;
        _player = player;
        _state = state;
    }

    public void Update(World world, float dt)
    {
        var ctrl = _player.Get<TopDownController>();
        if (ctrl == null) return;

        var move = Vector2.Zero;
        var input = _app.Input;
        if (input.IsActionDown(GameAction.MoveUp)) move.Y -= 1;
        if (input.IsActionDown(GameAction.MoveDown)) move.Y += 1;
        if (input.IsActionDown(GameAction.MoveLeft)) move.X -= 1;
        if (input.IsActionDown(GameAction.MoveRight)) move.X += 1;

        ctrl.InputDirection = move;
        ctrl.SpeedMultiplier = _state.PlayerMoveSpeedMultiplier;
    }
}