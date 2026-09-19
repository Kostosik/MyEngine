using MyEngine;
using MyEngine.Ecs;
using MyEngine.Scenes;

namespace Tests;

public class SceneTests
{
    // === Тестовая сцена ===

    private class TestScene : Scene
    {
        public bool Loaded;
        public bool Unloaded;
        public int UpdateCount;
        public int VariableUpdateCount;
        public int RenderCount;
        public int ImGuiCount;

        public override void OnLoad(Application app) => Loaded = true;
        public override void OnUnload(Application app) => Unloaded = true;
        public override void Update(Application app, float dt) => UpdateCount++;
        public override void UpdateVariable(Application app, float dt) => VariableUpdateCount++;
        public override void Render(Application app) => RenderCount++;
        public override void OnImGui(Application app) => ImGuiCount++;
    }

    // Заглушка Application для тестов — мы не можем создать настоящий
    // (ему нужно окно). Обойдёмся через null и проверки там, где он не нужен.
    // В SceneManager.Attach принимает Application, но большинство операций
    // работают без него.

    private static SceneManager MakeManager(params (string name, Func<Scene> factory)[] scenes)
    {
        var m = new SceneManager();
        foreach (var (name, factory) in scenes)
            m.Register(name, factory);
        return m;
    }

    // ============================================================
    // Регистрация
    // ============================================================

    [Fact]
    public void Register_AddsToRegistry()
    {
        var m = MakeManager(("test", () => new TestScene()));
        Assert.Contains("test", m.RegisteredScenes);
    }

    [Fact]
    public void Load_UnknownScene_Throws()
    {
        var m = MakeManager();
        Assert.Throws<InvalidOperationException>(() => m.Load("nope"));
    }

    // ============================================================
    // Load (replace)
    // ============================================================

    [Fact]
    public void Load_CreatesSceneAndCallsOnLoad()
    {
        TestScene? captured = null;
        var m = MakeManager(("test", () => captured = new TestScene()));

        m.Load("test");
        m.Update(0.016f);

        Assert.NotNull(captured);
        Assert.True(captured!.Loaded);
        Assert.Equal(1, m.StackCount);
        Assert.Same(captured, m.Current);
    }

    [Fact]
    public void Load_Twice_UnloadsFirstScene()
    {
        var scenes = new List<TestScene>();
        var m = MakeManager(("a", () => { var s = new TestScene(); scenes.Add(s); return s; }
        ),
                            ("b", () => { var s = new TestScene(); scenes.Add(s); return s; }
        ));

        m.Load("a");
        m.Update(0.016f);

        m.Load("b");
        m.Update(0.016f);

        Assert.Equal(2, scenes.Count);
        Assert.True(scenes[0].Loaded);
        Assert.True(scenes[0].Unloaded, "Первую сцену должны выгрузить");
        Assert.True(scenes[1].Loaded);
        Assert.Equal(1, m.StackCount);
        Assert.Same(scenes[1], m.Current);
    }

    // ============================================================
    // Push / Pop
    // ============================================================

    [Fact]
    public void Push_AddsToStack()
    {
        var a = new TestScene();
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);

        Assert.Equal(2, m.StackCount);
        Assert.Same(b, m.Current);
    }

    [Fact]
    public void Pop_RemovesTopScene()
    {
        var a = new TestScene();
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);
        m.Pop();
        m.Update(0.016f);

        Assert.Equal(1, m.StackCount);
        Assert.Same(a, m.Current);
        Assert.True(b.Unloaded, "Верхняя сцена должна быть выгружена");
    }

    [Fact]
    public void Pop_EmptyStack_DoesNothing()
    {
        var m = MakeManager();
        m.Pop();
        m.Update(0.016f);

        Assert.Equal(0, m.StackCount);
        Assert.Null(m.Current);
    }

    // ============================================================
    // Тики
    // ============================================================

    [Fact]
    public void Update_TicksCurrentScene()
    {
        var s = new TestScene();
        var m = MakeManager(("test", () => s));

        m.Load("test");
        m.Update(0.016f);
        m.Update(0.016f);
        m.Update(0.016f);

        Assert.Equal(3, s.UpdateCount);
    }

    [Fact]
    public void UpdateVariable_TicksCurrentScene()
    {
        var s = new TestScene();
        var m = MakeManager(("test", () => s));

        m.Load("test");
        m.Update(0.016f);
        m.UpdateVariable(0.016f);
        m.UpdateVariable(0.016f);

        Assert.Equal(2, s.VariableUpdateCount);
    }

    [Fact]
    public void Update_OnlyTicksTopScene_WhenPauseBelow()
    {
        var a = new TestScene { PauseBelow = true };
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);
        m.Update(0.016f);

        // a замерла после push
        Assert.Equal(1, a.UpdateCount);
        // b тикает
        Assert.Equal(2, b.UpdateCount);
    }

    [Fact]
    public void Update_BothTick_WhenPauseBelowFalse()
    {
        var a = new TestScene { PauseBelow = false };
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);

        Assert.Equal(2, a.UpdateCount);
        Assert.Equal(1, b.UpdateCount);
    }

    // ============================================================
    // Render
    // ============================================================

    [Fact]
    public void Render_OnlyTop_Renders()
    {
        var a = new TestScene { RenderBelow = false };
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);
        m.Render();

        Assert.Equal(0, a.RenderCount);
        Assert.Equal(1, b.RenderCount);
    }

    [Fact]
    public void Render_BothRender_WhenRenderBelowTrue()
    {
        var a = new TestScene { RenderBelow = true };
        var b = new TestScene();
        var m = MakeManager(("a", () => a), ("b", () => b));

        m.Load("a");
        m.Update(0.016f);
        m.Push("b");
        m.Update(0.016f);
        m.Render();

        Assert.Equal(1, a.RenderCount);
        Assert.Equal(1, b.RenderCount);
    }
}