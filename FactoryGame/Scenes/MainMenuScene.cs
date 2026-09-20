using MyEngine;
using MyEngine.Scenes;
using FactoryGame.UI;
using System.Numerics;

namespace FactoryGame.Scenes;

public sealed class MainMenuScene : Scene
{
    private MainMenuScreen? _screen;

    public MainMenuScene() { Name = "MainMenu"; }

    public override void OnLoad(Application app)
    {
        _screen = new MainMenuScreen();
        _screen.PlayRequested += () => app.Scenes.Load("Game");
        _screen.QuitRequested += () => app.RequestClose();
        app.UI.Add(_screen);
    }

    public override void OnUnload(Application app)
    {
        if (_screen != null) { app.UI.Remove(_screen); _screen = null; }
    }

    public override void Render(Application app)
    {
        app.GL.ClearColor(0.05f, 0.07f, 0.12f, 1f);
        app.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        var proj = Matrix4x4.CreateOrthographicOffCenter(
            0, app.Width, app.Height, 0, -1f, 1f);

        app.UIRounded.Begin(proj);
        app.UI.DrawBackground();
        app.UIRounded.End();

        app.Batch.Begin(proj);
        app.UI.DrawForeground();
        app.Batch.End();

        app.GL.Disable(Silk.NET.OpenGL.EnableCap.Blend);
    }
}