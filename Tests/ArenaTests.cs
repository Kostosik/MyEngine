using MyEngine.Memory;

namespace Tests;

public class ArenaTests
{
    [Fact]
    public void Alloc_ReturnsDistinctMemory()
    {
        var arena = new Arena(1024);
        var a = arena.Alloc<int>(4);
        var b = arena.Alloc<int>(4);
        a[0] = 42;
        Assert.Equal(42, a[0]);
        Assert.Equal(0, b[0]);
        Assert.True(arena.Used >= 32);
    }

    [Fact]
    public void Reset_ReclaimsAllSpace()
    {
        var arena = new Arena(1024);
        arena.Alloc<int>(100);
        arena.Reset();
        Assert.Equal(0, arena.Used);
    }

    [Fact]
    public void Alloc_AlignsTo8Bytes()
    {
        var arena = new Arena(1024);
        arena.AllocBytes(3);          // 3 → выравнивание до 8
        var span = arena.AllocBytes(1);
        // второй кусок должен начинаться с выровненного смещения
        Assert.True(arena.Used >= 9);
    }

    [Fact]
    public void Grow_WhenExceeded()
    {
        var arena = new Arena(64);
        arena.Alloc<int>(100); // 400 байт > 64
        Assert.True(arena.Capacity >= 400);
    }
}