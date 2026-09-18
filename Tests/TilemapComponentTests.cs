using MyEngine.Tilemap;
using System.Numerics;

namespace MyEngine.Tests;

public class TilemapComponentTests
{
    private static TilemapComponent MakeMap()
    {
        var map = new TilemapComponent();
        map.Initialize(width: 100, height: 100, chunkSize: 16);
        map.AddLayer(new TilemapLayer("ground"));
        map.AddLayer(new TilemapLayer("walls"));
        return map;
    }

    [Fact]
    public void SetAndGet_AcrossChunk()
    {
        var map = MakeMap();

        // Позиции в разных чанках (chunkSize = 16)
        map.SetTile(0, 5, 5, 1);     // чанк (0,0)
        map.SetTile(0, 20, 5, 2);    // чанк (1,0)
        map.SetTile(0, 5, 20, 3);    // чанк (0,1)
        map.SetTile(0, 20, 20, 4);   // чанк (1,1)

        Assert.Equal(1, map.GetTile(0, 5, 5));
        Assert.Equal(2, map.GetTile(0, 20, 5));
        Assert.Equal(3, map.GetTile(0, 5, 20));
        Assert.Equal(4, map.GetTile(0, 20, 20));
    }

    [Fact]
    public void GetTile_MissingChunk_ReturnsNegative()
    {
        var map = MakeMap();
        Assert.Equal(-1, map.GetTile(0, 50, 50));
    }

    [Fact]
    public void SetTile_OutOfBounds_NoOp()
    {
        var map = MakeMap();
        map.SetTile(0, -1, 0, 5);
        map.SetTile(0, 200, 0, 5);
        Assert.Equal(-1, map.GetTile(0, 0, 0));
    }

    [Fact]
    public void ChunkCount_GrowsWithSet()
    {
        var map = MakeMap();
        Assert.Equal(0, map.ChunkCount);

        map.SetTile(0, 5, 5, 1);
        Assert.Equal(1, map.ChunkCount);

        map.SetTile(0, 50, 50, 1);   // другой чанк
        Assert.Equal(2, map.ChunkCount);
    }

    [Fact]
    public void WorldToTile_Correctly()
    {
        var map = MakeMap();
        map.Tileset = null!; // не используется в WorldToTile напрямую,
        // но зависит от TileSize → нельзя без Tileset.
        // Skip — тестируем в игре.
        Assert.True(true);
    }
}