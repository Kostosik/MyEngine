using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// Корневой UI-контейнер. Содержит один или несколько экранов,
/// обновляет ввод, рисует элементы поверх сцены.
/// </summary>
public sealed class UIRoot
{
    private readonly List<UIElement> _screens = new();
    private readonly UIRenderContext _ctx = new();

    public Font Font
    {
        get => _ctx.Font;
        set
        {
            _ctx.Font = value;
            FontProvider.Set(value);   // ← связь с глобальным
        }
    }

    public UIRoundedRenderer Rounded
    {
        get => _ctx.Rounded;
        set => _ctx.Rounded = value;
    }

    public void DrawBackground()
    {
        foreach (var s in _screens)
            s.DrawBackground(_ctx);
    }

    public void DrawForeground()
    {
        foreach (var s in _screens)
            s.DrawForeground(_ctx);
    }
    public Texture2D White
    {
        get => _ctx.White;
        set => _ctx.White = value;
    }

    public SpriteBatch Batch
    {
        get => _ctx.Batch;
        set => _ctx.Batch = value;
    }

    public IReadOnlyList<UIElement> Screens => _screens;

    public UIElement Add(UIElement screen)
    {
        _screens.Add(screen);
        return screen;
    }

    public void Remove(UIElement screen) => _screens.Remove(screen);
    public void Clear() => _screens.Clear();

    public void Update(Vector2 mousePos, bool mouseDown, bool mouseJustPressed, int screenWidth, int screenHeight)
    {
        _ctx.ScreenWidth = screenWidth;
        _ctx.ScreenHeight = screenHeight;

        var root = new UIRect(0, 0, screenWidth, screenHeight);
        foreach (var screen in _screens)
        {
            screen.Layout(root);
            screen.Update(mousePos, mouseDown, mouseJustPressed);
        }
    }


    /// <summary>Возвращает true, если курсор над каким-то UI-элементом — блокирует игровой ввод.</summary>
    public bool IsMouseOver(Vector2 mousePos)
    {
        foreach (var screen in _screens)
            if (screen.Visible && screen.Bounds.Contains(mousePos))
                return true;
        return false;
    }
}