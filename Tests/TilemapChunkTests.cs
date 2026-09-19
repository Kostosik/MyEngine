using MyEngine.Tilemap;

namespace Tests;

public class TilemapChunkTests
{
    [Fact]
    public void Chunk_StartsEmpty()
    {
        var chunk = new TilemapChunk(size: 4, layerCount: 2);
        for (int l = 0; l < 2; l++)
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    Assert.Equal(-1, chunk.Get(l, x, y));
    }

    [Fact]
    public void Chunk_SetAndGet()
    {
        var chunk = new TilemapChunk(size: 4, layerCount: 2);
        chunk.Set(layer: 1, lx: 2, ly: 3, tile: 5);
        Assert.Equal(5, chunk.Get(1, 2, 3));
    }

    [Fact]
    public void Chunk_OutOfBounds_ReturnsNegative()
    {
        var chunk = new TilemapChunk(size: 4, layerCount: 1);
        Assert.Equal(-1, chunk.Get(0, -1, 0));
        Assert.Equal(-1, chunk.Get(0, 4, 0));
    }

    [Fact]
    public void Chunk_InvalidLayer_ReturnsNegative()
    {
        var chunk = new TilemapChunk(size: 4, layerCount: 1);
        Assert.Equal(-1, chunk.Get(layer: 5, 0, 0));
    }
}