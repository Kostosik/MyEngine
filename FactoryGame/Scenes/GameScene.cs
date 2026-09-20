using FactoryGame.States;
using MyEngine;
using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Effects;
using MyEngine.InputEngine;
using MyEngine.Rendering;
using MyEngine.Scenes;
using MyEngine.Systems;
using System.Numerics;

namespace FactoryGame.Scenes;

public sealed class GameScene : Scene
{
    private SpriteRenderSystem _spriteRenderer = null!;
    private GameContext _ctx = null!;
    private Entity _player = null!;
    private MyEngine.Tilemap.TilemapComponent _tilemap = null!;
    public GameScene() { Name = "Game"; }

    public override void OnLoad(Application app)
    {
        _spriteRenderer = new SpriteRenderSystem(app.Batch);

        // Мир и контекст
        ResetWorld();
        _ctx = new GameContext
        {
            App = app,
            World = World,
            Camera = app.Camera,
            Events = app.Events,
            Particles = app.Particles,
            Font = app.Font,
            UpdateSystems = UpdateSystems,
            VariableSystems = VariableSystems,
            Profiler = app.Profiler,
            Restart = () => app.Scenes.Load("Game"),
            FinishLoading = OnLoadingComplete,
        };
        Context = _ctx;

        // Состояние
        World.SetSingleton(new GameState());
        World.AttachEvents(app.Events);
        _ctx.State = World.GetSingleton<GameState>()!;

        IsLoading = false;   // загрузки пока нет

        OnLoadingComplete();
    }

    protected override bool IsLoadingComplete() => true;

    protected override void OnLoadingComplete()
    {
        var worldPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Maps", "world.json");
        _tilemap = MyEngine.Tilemap.TilemapLoader.Load(Context.App.GL, worldPath);

        var mapEntity = World.Create();
        mapEntity.Add(new Transform { Position = Vector2.Zero });
        mapEntity.Add(_tilemap);

        // Игрок
        _player = CreatePlayer(World, new Vector2(500, 500));
        _ctx.Player = _player;

        // Камера
        Context.Camera.Position = _player.Get<Transform>()!.Position;

        // Системы
        UpdateSystems.Add(new MovementSystem(Context.App.Jobs));
        UpdateSystems.Add(new TopDownControllerSystem());
        UpdateSystems.Add(new TilemapCollisionSystem(_tilemap));
        VariableSystems.Add(new FactoryGame.Systems.PlayerInputSystem(Context.App, _player));
        VariableSystems.Add(new MyEngine.Systems.TopDownControllerSystem());

        Log.Info("Factory", $"Player #{_player.Id} at ({_player.Get<Transform>()!.Position})");
    }

    public override void Update(Application app, float dt)
    {
        _ctx.State.Update(dt);
        UpdateSystems.Update(World, dt);
    }

    public override void UpdateVariable(Application app, float dt)
    {
        TickLoading();
        if (IsLoading) return;

        VariableSystems.Update(World, dt);

        var t = _player.Get<Transform>();
        if (t != null)
        {
            Context.Camera.Follow(t.Position, Vector2.Zero, dt);
            Context.Camera.ApplyBounds(app.Width, app.Height);
            Context.Camera.UpdateShake(dt);
        }
    }

    public override void Render(Application app)
    {
        app.GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
        app.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit);

        var proj = Context.Camera.GetProjection(app.Width, app.Height);
        app.Batch.Begin(proj);
        MyEngine.Tilemap.TilemapRenderer.Draw(app.Batch, World, Context.Camera, app.Width, app.Height);
        _spriteRenderer.Draw(World);
        ParticleRenderer.Draw(app.Batch, app.Particles);
        app.Batch.End();

        // UI поверх
        var screenProj = Matrix4x4.CreateOrthographicOffCenter(
            0, app.Width, app.Height, 0, -1f, 1f);
        app.UIRounded.Begin(screenProj);
        app.UI.DrawBackground();
        app.UIRounded.End();

        app.Batch.Begin(screenProj);
        app.UI.DrawForeground();
        app.Batch.End();

        app.GL.Disable(Silk.NET.OpenGL.EnableCap.Blend);
    }

    public override void OnUnload(Application app)
    {
        // пока нечего освобождать
    }

    private Entity CreatePlayer(World world, Vector2 position)
    {
        var e = world.Create();
        e.Add(new PlayerTag());
        e.Add(new Transform { Position = position });
        e.Add(new Velocity());
        e.Add(new Collider
        {
            Size = new Vector2(24, 24),
            IsStatic = false,
            Layer = MyEngine.Physics.Layer.Player,
            CollidesWith = MyEngine.Physics.Layer.Wall
        });
        e.Add(new Sprite
        {
            Size = new Vector2(24, 24),
            Color = new Vector4(0.3f, 0.8f, 0.4f, 1f)
        });
        e.Add(new TopDownController { MaxSpeed = 250f });
        return e;
    }
}