using MyEngine.GameFlow;
using Silk.NET.Input;

namespace TestRPGGame.States;

public sealed class DialogueState : GameStateBase
{
    private readonly GameContext _ctx;

    public DialogueState(GameContext ctx) => _ctx = ctx;

    public override void Update(float dt)
    {
        if (!_ctx.Dialogue.IsOpen)
        {
            _ctx.App.StateMachine.RequestTransition("Playing");
            return;
        }

        // Первый кадр после открытия — глотаем ввод, чтобы не скипнуть фразу
        if (_ctx.Dialogue.JustOpened)
        {
            _ctx.Dialogue.ClearJustOpened();
            return;
        }

        var runner = _ctx.Dialogue.Runner;
        if (runner?.CurrentNode == null) return;

        // Escape — закрыть
        if (_ctx.App.Input.WasPressed(Key.Escape))
        {
            _ctx.Dialogue.Close();
            return;
        }

        var node = runner.CurrentNode;

        if (node.HasChoices)
        {
            // Выбор — цифрами 1..9
            var numberKeys = new[]
            {
                Key.Number1, Key.Number2, Key.Number3,
                Key.Number4, Key.Number5, Key.Number6,
                Key.Number7, Key.Number8, Key.Number9
            };

            int count = runner.AvailableChoices.Count;
            for (int i = 0; i < count && i < numberKeys.Length; i++)
            {
                if (_ctx.App.Input.ConsumePressed(numberKeys[i]))
                {
                    runner.Choose(i);
                    return;
                }
            }
        }
        else
        {
            // Обычный узел — E → далее
            if (_ctx.App.Input.ConsumePressed(Key.E))
            {
                runner.Advance();
            }
        }
    }
}