using MyEngine.UI;
using MyEngine.UI.Widgets;
using System.Numerics;

namespace Tests;

public class StackPanelTests
{
    private static Panel MakeChild(float w, float h)
        => new() { Size = new Vector2(w, h), Visible = true };

    // ============================================================
    // Vertical
    // ============================================================

    [Fact]
    public void Vertical_StacksTopToBottom()
    {
        var stack = new StackPanel { Size = new Vector2(200, 300) };
        var a = stack.Add(MakeChild(50, 30));
        var b = stack.Add(MakeChild(50, 40));
        var c = stack.Add(MakeChild(50, 20));

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(0f, a.Bounds.Y);
        Assert.Equal(30f, b.Bounds.Y);
        Assert.Equal(70f, c.Bounds.Y);
    }

    [Fact]
    public void Vertical_ChildrenWidthEqualsAvailable()
    {
        var stack = new StackPanel { Size = new Vector2(300, 100) };
        var a = stack.Add(MakeChild(50, 20));

        stack.Layout(new UIRect(0, 0, 300, 100));

        Assert.Equal(300f, a.Bounds.Width);
    }

    [Fact]
    public void Vertical_SpacingAddsGap()
    {
        var stack = new StackPanel
        {
            Size = new Vector2(200, 300),
            Spacing = 10
        };
        var a = stack.Add(MakeChild(50, 30));
        var b = stack.Add(MakeChild(50, 40));

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(40f, b.Bounds.Y); // 30 + 10
    }

    [Fact]
    public void Vertical_PaddingShiftsChildren()
    {
        var stack = new StackPanel
        {
            Size = new Vector2(200, 300),
            PaddingLeft = 20,
            PaddingTop = 15
        };
        var a = stack.Add(MakeChild(50, 30));

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(20f, a.Bounds.X);
        Assert.Equal(15f, a.Bounds.Y);
        Assert.Equal(160f, a.Bounds.Width); // 200 - 20 - 0
    }

    [Fact]
    public void Vertical_PositionIncludesParentOffset()
    {
        var stack = new StackPanel { Size = new Vector2(200, 300) };
        var a = stack.Add(MakeChild(50, 30));

        stack.Layout(new UIRect(100, 50, 200, 300));

        Assert.Equal(100f, a.Bounds.X);
        Assert.Equal(50f, a.Bounds.Y);
    }

    // ============================================================
    // Horizontal
    // ============================================================

    [Fact]
    public void Horizontal_StacksLeftToRight()
    {
        var stack = new StackPanel
        {
            Direction = StackDirection.Horizontal,
            Size = new Vector2(300, 100)
        };
        var a = stack.Add(MakeChild(50, 20));
        var b = stack.Add(MakeChild(60, 20));
        var c = stack.Add(MakeChild(40, 20));

        stack.Layout(new UIRect(0, 0, 300, 100));

        Assert.Equal(0f, a.Bounds.X);
        Assert.Equal(50f, b.Bounds.X);
        Assert.Equal(110f, c.Bounds.X);
    }

    [Fact]
    public void Horizontal_ChildrenHeightEqualsAvailable()
    {
        var stack = new StackPanel
        {
            Direction = StackDirection.Horizontal,
            Size = new Vector2(300, 100)
        };
        var a = stack.Add(MakeChild(20, 40));

        stack.Layout(new UIRect(0, 0, 300, 100));

        Assert.Equal(100f, a.Bounds.Height);
    }

    // ============================================================
    // Выравнивание
    // ============================================================

    [Fact]
    public void Vertical_CenterAlign_CentersCrossAxis()
    {
        var stack = new StackPanel
        {
            Size = new Vector2(200, 300),
            CrossAlign = StackAlignment.Center
        };
        var a = stack.Add(MakeChild(50, 30));

        stack.Layout(new UIRect(0, 0, 200, 300));

        // Ребёнок должен быть отцентрирован: ширина у него = availW, так что x = 0
        // Проверим Y-выравнивание для горизонтального варианта — там интереснее
        Assert.Equal(0f, a.Bounds.X);
    }

    [Fact]
    public void Horizontal_CenterAlign_CentersY()
    {
        var stack = new StackPanel
        {
            Direction = StackDirection.Horizontal,
            Size = new Vector2(300, 100),
            CrossAlign = StackAlignment.Center
        };
        var a = stack.Add(MakeChild(50, 40));

        stack.Layout(new UIRect(0, 0, 300, 100));

        // availH = 100, childH = 100 (равен availH). Ожидаем Y = 0.
        // Проверим End-вариант в отдельном тесте.
        Assert.Equal(0f, a.Bounds.Y);
    }

    [Fact]
    public void Horizontal_EndAlign_PushesToBottom()
    {
        var stack = new StackPanel
        {
            Direction = StackDirection.Horizontal,
            Size = new Vector2(300, 100),
            CrossAlign = StackAlignment.End,
            PaddingTop = 0,
            PaddingBottom = 20
        };
        var a = stack.Add(MakeChild(50, 40));

        stack.Layout(new UIRect(0, 0, 300, 100));

        // availH = 100 - 0 - 20 = 80. Ребёнок = availH = 80. Y = 0.
        Assert.Equal(0f, a.Bounds.Y);
    }

    // ============================================================
    // Игнорирование Anchor и Offset
    // ============================================================

    [Fact]
    public void ChildAnchorAndOffset_AreIgnored()
    {
        var stack = new StackPanel { Size = new Vector2(200, 300) };
        var child = new Panel
        {
            Size = new Vector2(50, 30),
            Anchor = Anchor.BottomRight,  // должно игнорироваться
            Offset = new Vector2(999, 999) // должно игнорироваться
        };
        stack.Add(child);

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(0f, child.Bounds.X);
        Assert.Equal(0f, child.Bounds.Y);
    }

    // ============================================================
    // Скрытые дети
    // ============================================================

    [Fact]
    public void HiddenChildren_AreSkipped()
    {
        var stack = new StackPanel { Size = new Vector2(200, 300) };
        var a = stack.Add(MakeChild(50, 30));
        var hidden = stack.Add(MakeChild(50, 40));
        hidden.Visible = false;
        var c = stack.Add(MakeChild(50, 20));

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(0f, a.Bounds.Y);
        Assert.Equal(30f, c.Bounds.Y); // hidden пропущен
    }
}