using MyEngine;
using MyEngine.Effects;
using MyEngine.Rendering;
using MyEngine.UI;
using FactoryGame.Scenes;
using System.Numerics;

namespace FactoryGame;

/// <summary>
/// Точка входа FactoryGame. Создаёт ресурсы приложения,
/// регистрирует сцены, запускает меню.
/// </summary>
public sealed class FactoryGameApp : Application
{
    public FactoryGameApp() : base("Factory Game", 1280, 720) { }

    protected override void Load()
    {
        // === Ресурсы приложения ===
        Batch = new SpriteBatch(GL);
        Particles = new ParticleSystem(capacity: 4096);
        Font = new Font(GL, Assets.PathFont("main.ttf"), 16f);

        Camera.DeadzoneSize = new Vector2(80, 60);
        Camera.LookAheadDistance = new Vector2(60, 0);
        Camera.LookAheadSmoothing = 4f;
        Camera.FollowSmoothing = 6f;
        Camera.Bounds = new MyEngine.Math.Aabb(
            new Vector2(0, 0),
            new Vector2(2000, 2000));
        Camera.ScreenWidth = Width;
        Camera.ScreenHeight = Height;

        UI.Batch = Batch;
        UI.Font = Font;
        UI.White = Texture2D.White(GL);
        UI.Rounded = UIRounded;

        // === Сцены ===
        Scenes.Register<MainMenuScene>("MainMenu");
        Scenes.Register<GameScene>("Game");

        Scenes.Load("MainMenu");
    }
}