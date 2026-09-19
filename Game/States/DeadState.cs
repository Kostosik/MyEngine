using MyEngine.GameFlow;
using MyEngine.InputEngine;

namespace TestRPGGame.States;

public sealed class DeadState : GameStateBase
{
    private readonly GameContext _ctx;

    public DeadState(GameContext ctx) => _ctx = ctx;

    public override void Enter()
    {
        _ctx.State.Phase = GamePhase.Dead;
    }

    public override void Update(float dt)
    {
        if (_ctx.App.Input.WasActionPressed(GameAction.Restart) || _ctx.GameOver.RestartRequested)
        {
            _ctx.GameOver.ClearRequest();
            _ctx.Restart();
        }
    }
}