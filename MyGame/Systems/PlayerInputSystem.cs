using MyEngine;
using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.InputEngine;
using System.Numerics;

namespace MyGame.Systems;

/// <summary>
/// Читает ввод игрока и пишет в TopDownController.
/// Специфичен для FactoryGame — без GameState, без бонусов.
/// Движение и применение скорости — в TopDownControllerSystem (движок).
/// </summary>
public sealed class PlayerInputSystem : ISystem
{
    private readonly Application _app;
    private readonly Entity _player;

    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 0;

    public PlayerInputSystem(Application app, Entity player)
    {
        _app = app;
        _player = player;
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
        // SpeedMultiplier не трогаем — он по умолчанию 1.0
    }
}