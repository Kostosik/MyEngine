using MyEngine.Ai;
using System.Numerics;

namespace MyEngine.Tests;

public class AStarTests
{
    [Fact]
    public void StraightPath()
    {
        var grid = new NavGrid(20, 20, 1f, Vector2.Zero);
        var path = AStar.FindPath(grid, new Vector2(0.5f, 0.5f), new Vector2(19.5f, 0.5f));
        Assert.NotNull(path);
        Assert.True(path!.Count >= 2);
    }

    [Fact]
    public void BlockedNoPath()
    {
        var grid = new NavGrid(20, 20, 1f, Vector2.Zero);
        // Стена во весь столбец x=10
        for (int y = 0; y < 20; y++)
            grid.SetWalkable(10, y, false);

        var path = AStar.FindPath(grid, new Vector2(0.5f, 0.5f), new Vector2(19.5f, 0.5f));
        Assert.Null(path);
    }

    [Fact]
    public void GoesAroundWall()
    {
        var grid = new NavGrid(20, 20, 1f, Vector2.Zero);
        // Стена x=10, но не до конца — проход в самом низу
        for (int y = 0; y < 19; y++)
            grid.SetWalkable(10, y, false);

        var path = AStar.FindPath(grid, new Vector2(0.5f, 0.5f), new Vector2(19.5f, 0.5f));
        Assert.NotNull(path);
        // Путь должен пройти через y ~19 (обход снизу)
        Assert.Contains(path!, p => p.Y > 15f);
    }

    [Fact]
    public void StartOnBlocked_ReturnsNull()
    {
        var grid = new NavGrid(20, 20, 1f, Vector2.Zero);
        grid.SetWalkable(0, 0, false);
        var path = AStar.FindPath(grid, new Vector2(0.5f, 0.5f), new Vector2(19.5f, 0.5f));
        Assert.Null(path);
    }
}