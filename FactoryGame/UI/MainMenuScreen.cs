using MyEngine.UI;
using MyEngine.UI.Widgets;
using System.Numerics;

namespace FactoryGame.UI;

public sealed class MainMenuScreen : Panel
{
    public event Action? PlayRequested;
    public event Action? QuitRequested;

    public MainMenuScreen()
    {
        Anchor = Anchor.StretchAll;
        MarginLeft = MarginRight = MarginTop = MarginBottom = 0;
        BackgroundTop = new Vector4(0.05f, 0.07f, 0.12f, 1f);
        BackgroundBottom = new Vector4(0.05f, 0.07f, 0.12f, 1f);
        BorderThickness = 0;

        Add(new Label
        {
            Text = "FACTORY",
            Anchor = Anchor.TopCenter,
            Offset = new Vector2(0, 120),
            Size = new Vector2(700, 60),
            Color = new Vector4(0.9f, 0.7f, 0.4f, 1f),
            Scale = 2f,
            Align = TextAlign.Center
        });

        Add(new Label
        {
            Text = "build · automate · expand",
            Anchor = Anchor.TopCenter,
            Offset = new Vector2(0, 200),
            Size = new Vector2(700, 30),
            Color = new Vector4(0.7f, 0.75f, 0.85f, 1f),
            Scale = 1.2f,
            Align = TextAlign.Center
        });

        var playBtn = Add(new Button
        {
            Text = "Play",
            Anchor = Anchor.MiddleCenter,
            Offset = new Vector2(0, -40),
            Size = new Vector2(240, 50)
        });
        playBtn.OnClick += _ => PlayRequested?.Invoke();

        var quitBtn = Add(new Button
        {
            Text = "Quit",
            Anchor = Anchor.MiddleCenter,
            Offset = new Vector2(0, 30),
            Size = new Vector2(240, 50)
        });
        quitBtn.OnClick += _ => QuitRequested?.Invoke();
    }
}