using MyEngine.Components;
using TestRPGGame.Components;
using MyEngine.GameFlow;
using MyEngine.Time;

namespace TestRPGGame.States;

public sealed class PlayingState : GameStateBase
{
    private readonly GameContext _ctx;

    public PlayingState(GameContext ctx) => _ctx = ctx;

    public override void Update(float dt)
    {
        if (_ctx.Dialogue.IsOpen)
        {
            _ctx.App.StateMachine.RequestTransition("Dialogue");
            return;
        }

        if (_ctx.Menu.IsOpen) return;

        _ctx.UpdateSystems.Update(_ctx.World, dt,
            scope: s => _ctx.Profiler.Measure(s.GetType().Name));

        _ctx.Popups.Update(dt);
        _ctx.Particles.Update(dt);

        var hp = _ctx.Player.Get<Health>();
        if (hp != null && !hp.IsAlive)
            _ctx.App.StateMachine.RequestTransition("Dead");
    }

    public override void UpdateVariable(float dt)
    {
        if (_ctx.Menu.IsOpen) return;
        if (_ctx.Dialogue.IsOpen) return;



        _ctx.VariableSystems.Update(_ctx.World, dt,
            scope: s => _ctx.Profiler.Measure("V:" + s.GetType().Name));

        var t = _ctx.Player.Get<Transform>();
        var v = _ctx.Player.Get<Velocity>();
        if (t != null && v != null)
        {
            _ctx.Camera.Follow(t.Position, v.Value, dt);
            _ctx.Camera.ApplyBounds(_ctx.App.Width, _ctx.App.Height);
        }
        _ctx.Camera.UpdateShake(TimeEngine.UnscaledDeltaTime);
    }
}