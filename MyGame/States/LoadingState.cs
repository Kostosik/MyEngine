using MyEngine.GameFlow;
using MyEngine.Game.Spawning;

namespace MyGame.States;

//public sealed class LoadingState : GameStateBase
//{
//    private readonly GameContext _ctx;
//    private readonly Func<AsyncMapLoader> _getLoader;

//    public LoadingState(GameContext ctx, Func<AsyncMapLoader> getLoader)
//    {
//        _ctx = ctx;
//        _getLoader = getLoader;
//    }

//    public override void Update(float dt)
//    {
//        if (_getLoader().IsDone)
//        {
//            _ctx.FinishLoading();
//            _ctx.App.StateMachine.RequestTransition("Playing");
//        }
//    }
//}