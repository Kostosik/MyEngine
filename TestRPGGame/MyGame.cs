using MyEngine;
using MyEngine.Assets;
using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.Rendering;
using MyEngine.UI;
using TestRPGGame.Scenes;
using TestRPGGame.UI;

namespace TestRPGGame;

/// <summary>
/// Точка входа игры. Создаёт ресурсы приложения, регистрирует сцены,
/// запускает MainMenu. Вся игровая логика — в сценах.
/// </summary>
public sealed class MyGame : Application
{
    public MyGame() : base("Lighthouse Keeper", 1280, 720) { }

    protected override void Load()
    {
        // === Ресурсы приложения (один раз) ===

        Batch = new SpriteBatch(GL);
        Particles = new ParticleSystem(capacity: 4096);
        Font = new Font(GL, Assets.PathFont("main.ttf"), 20f);

        Camera.DeadzoneSize = new System.Numerics.Vector2(80, 60);
        Camera.LookAheadDistance = new System.Numerics.Vector2(60, 0);
        Camera.LookAheadSmoothing = 4f;
        Camera.FollowSmoothing = 6f;
        Camera.Bounds = new MyEngine.Math.Aabb(
            new System.Numerics.Vector2(0, 0),
            new System.Numerics.Vector2(2000, 2000));
        Camera.ScreenWidth = Width;
        Camera.ScreenHeight = Height;

        // UI ресурсы
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