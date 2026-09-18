using MyEngine.Ecs;
using MyEngine.Game.Components;
using MyEngine.Game.Systems;
using MyEngine.Game.UI;
using MyEngine.GameFlow;
using MyEngine.UI;

namespace MyEngine.Game.States;

/// <summary>
/// Доступ к игровым сервисам через GameContext.GetService.
/// Позволяет писать _ctx.Player() вместо _ctx.GetService&lt;Entity&gt;().
/// </summary>
public static class GameContextExtensions
{
    public static Entity? Player(this GameContext ctx)
        => ctx.GetService<Entity>();

    public static GameState? State(this GameContext ctx)
        => ctx.GetService<GameState>();

    public static DamagePopupSystem? Popups(this GameContext ctx)
        => ctx.GetService<DamagePopupSystem>();

    public static DialogueBox? Dialogue(this GameContext ctx)
        => ctx.GetService<DialogueBox>();

    public static LevelUpMenu? Menu(this GameContext ctx)
        => ctx.GetService<LevelUpMenu>();

    public static GameOverScreen? GameOver(this GameContext ctx)
        => ctx.GetService<GameOverScreen>();
}