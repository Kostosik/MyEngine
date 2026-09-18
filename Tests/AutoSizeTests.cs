using MyEngine.UI;
using MyEngine.UI.Widgets;
using System.Numerics;

namespace MyEngine.Tests;

public class AutoSizeTests
{
    private class FixedSizeElement : UIElement
    {
        public override Vector2 PreferredSize => new Vector2(120, 40);
    }

    // ============================================================
    // ResolveSize
    // ============================================================

    [Fact]
    public void ResolveSize_WithoutAutoSize_UsesSize()
    {
        var e = new FixedSizeElement { Size = new Vector2(50, 30) };
        var r = e.ResolveSize();
        Assert.Equal(50f, r.X);
        Assert.Equal(30f, r.Y);
    }

    [Fact]
    public void ResolveSize_AutoSizeX_UsesPreferred()
    {
        var e = new FixedSizeElement
        {
            Size = new Vector2(50, 30),
            AutoSizeX = true
        };
        var r = e.ResolveSize();
        Assert.Equal(120f, r.X);   // Preferred.X
        Assert.Equal(30f, r.Y);    // Size.Y
    }

    [Fact]
    public void ResolveSize_AutoSizeY_UsesPreferred()
    {
        var e = new FixedSizeElement
        {
            Size = new Vector2(50, 30),
            AutoSizeY = true
        };
        var r = e.ResolveSize();
        Assert.Equal(50f, r.X);    // Size.X
        Assert.Equal(40f, r.Y);    // Preferred.Y
    }

    [Fact]
    public void ResolveSize_PrefSmallerThanSize_UsesSize()
    {
        var e = new FixedSizeElement
        {
            Size = new Vector2(200, 100),
            AutoSizeX = true,
            AutoSizeY = true
        };
        var r = e.ResolveSize();
        Assert.Equal(200f, r.X);   // max(120, 200)
        Assert.Equal(100f, r.Y);   // max(40, 100)
    }

    // ============================================================
    // StackPanel + auto-size
    // ============================================================

    [Fact]
    public void StackPanel_Vertical_HonorsChildPreferredHeight()
    {
        var stack = new StackPanel { Size = new Vector2(200, 300) };

        var a = stack.Add(new FixedSizeElement
        {
            Size = new Vector2(0, 0),
            AutoSizeY = true      // высота из PreferredSize = 40
        });
        var b = stack.Add(new FixedSizeElement
        {
            Size = new Vector2(0, 30),
            AutoSizeY = false     // высота = 30
        });

        stack.Layout(new UIRect(0, 0, 200, 300));

        Assert.Equal(0f, a.Bounds.Y);
        Assert.Equal(40f, a.Bounds.Height);

        Assert.Equal(40f, b.Bounds.Y);
        Assert.Equal(30f, b.Bounds.Height);
    }
}