using MyEngine.UI;
using MyEngine.UI.Widgets;
using System.Numerics;

namespace MyEngine.Tests;

public class GridPanelTests
{
    private static Panel MakeCell() => new() { Size = new Vector2(50, 50) };

    // ============================================================
    // Фиксированный размер ячейки
    // ============================================================

    [Fact]
    public void FixedCell_PositionsRowMajor()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 400),
            Columns = 3,
            CellSize = new Vector2(100, 50),
            Spacing = 10,
            Padding = 0
        };

        var cells = new UIElement[6];
        for (int i = 0; i < 6; i++) cells[i] = grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 400));

        // Row 0
        Assert.Equal(0f, cells[0].Bounds.X);
        Assert.Equal(0f, cells[0].Bounds.Y);

        Assert.Equal(110f, cells[1].Bounds.X);   // 100 + 10
        Assert.Equal(0f, cells[1].Bounds.Y);

        Assert.Equal(220f, cells[2].Bounds.X);   // 2*(100+10)
        Assert.Equal(0f, cells[2].Bounds.Y);

        // Row 1
        Assert.Equal(0f, cells[3].Bounds.X);
        Assert.Equal(60f, cells[3].Bounds.Y);    // 50 + 10

        Assert.Equal(110f, cells[4].Bounds.X);
        Assert.Equal(60f, cells[4].Bounds.Y);
    }

    [Fact]
    public void FixedCell_WidthHeightFromCellSize()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 400),
            Columns = 3,
            CellSize = new Vector2(80, 60),
        };

        var cell = grid.Add(MakeCell());
        grid.Layout(new UIRect(0, 0, 400, 400));

        Assert.Equal(80f, cell.Bounds.Width);
        Assert.Equal(60f, cell.Bounds.Height);
    }

    // ============================================================
    // Fit — растяжение
    // ============================================================

    [Fact]
    public void FitCells_SplitAvailableSpace()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 4,
            Spacing = 0,
            Padding = 0
        };

        var cells = new UIElement[4];
        for (int i = 0; i < 4; i++) cells[i] = grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 200));

        // 400 / 4 = 100 ширина, 1 row → 200 высота
        Assert.Equal(100f, cells[0].Bounds.Width);
        Assert.Equal(200f, cells[0].Bounds.Height);

        Assert.Equal(100f, cells[1].Bounds.X);
    }

    [Fact]
    public void FitCells_TwoRows()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 4,
            Spacing = 0,
            Padding = 0
        };

        for (int i = 0; i < 8; i++) grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 200));

        var cells = grid.Children.ToArray();
        // Row 0 Y = 0, row 1 Y = 100
        Assert.Equal(0f, cells[0].Bounds.Y);
        Assert.Equal(100f, cells[4].Bounds.Y);
    }

    // ============================================================
    // Padding и Spacing
    // ============================================================

    [Fact]
    public void Padding_ShiftsGrid()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 2,
            CellSize = new Vector2(50, 50),
            Padding = 20
        };

        var cell = grid.Add(MakeCell());
        grid.Layout(new UIRect(0, 0, 400, 200));

        Assert.Equal(20f, cell.Bounds.X);
        Assert.Equal(20f, cell.Bounds.Y);
    }

    [Fact]
    public void SpacingBetweenCells()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 2,
            CellSize = new Vector2(50, 50),
            Spacing = 15
        };

        var a = grid.Add(MakeCell());
        var b = grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 200));

        Assert.Equal(65f, b.Bounds.X);   // 50 + 15
    }

    // ============================================================
    // Rows
    // ============================================================

    [Fact]
    public void Rows_CalculatedFromChildCount()
    {
        var grid = new GridPanel { Size = new Vector2(400, 400), Columns = 4 };
        for (int i = 0; i < 9; i++) grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 400));

        Assert.Equal(3, grid.Rows);   // ceil(9/4) = 3
    }

    [Fact]
    public void Rows_ZeroWhenNoChildren()
    {
        var grid = new GridPanel { Columns = 4 };
        Assert.Equal(0, grid.Rows);
    }

    // ============================================================
    // Hidden cells
    // ============================================================

    [Fact]
    public void HiddenCells_AreSkipped()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 3,
            CellSize = new Vector2(100, 50)
        };

        var a = grid.Add(MakeCell());
        var hidden = grid.Add(MakeCell());
        hidden.Visible = false;
        var c = grid.Add(MakeCell());

        grid.Layout(new UIRect(0, 0, 400, 200));

        // Три ячейки, но hidden пропущен.
        // a в col 0, c в col 1 (а не col 2)
        Assert.Equal(0f, a.Bounds.X);
        Assert.Equal(100f, c.Bounds.X);
    }

    // ============================================================
    // CellAlign
    // ============================================================

    [Fact]
    public void CellAlign_Center_CentersChild()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 1,
            CellSize = new Vector2(100, 100),
            CellAlign = GridAlignment.Center
        };

        var child = grid.Add(new Panel { Size = new Vector2(40, 40) });

        grid.Layout(new UIRect(0, 0, 400, 200));

        // Центр ячейки (50, 50), ребёнок 40×40 → X = 30, Y = 30
        Assert.Equal(30f, child.Bounds.X);
        Assert.Equal(30f, child.Bounds.Y);
        Assert.Equal(40f, child.Bounds.Width);
    }

    [Fact]
    public void CellAlign_End_AlignsToBottomRight()
    {
        var grid = new GridPanel
        {
            Size = new Vector2(400, 200),
            Columns = 1,
            CellSize = new Vector2(100, 100),
            CellAlign = GridAlignment.End
        };

        var child = grid.Add(new Panel { Size = new Vector2(40, 40) });

        grid.Layout(new UIRect(0, 0, 400, 200));

        Assert.Equal(60f, child.Bounds.X);   // 100 - 40
        Assert.Equal(60f, child.Bounds.Y);
    }
}