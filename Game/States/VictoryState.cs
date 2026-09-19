using MyEngine.GameFlow;
using MyEngine.InputEngine;

namespace TestRPGGame.States;

public sealed class VictoryState : GameStateBase
{
    private readonly GameContext _ctx;

    public VictoryState(GameContext ctx) => _ctx = ctx;

    public override void Enter()
    {
        _ctx.State.Phase = GamePhase.Victory;
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