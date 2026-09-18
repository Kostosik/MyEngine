using MyEngine.Pooling;

namespace MyEngine.Tests;

public class PoolTests
{
    private class TestObj
    {
        public int Value;
    }

    [Fact]
    public void Rent_ReturnsNewObject()
    {
        var pool = new Pool<TestObj>(() => new TestObj());
        var obj = pool.Rent();
        Assert.NotNull(obj);
        Assert.Equal(1, pool.RentedCount);
    }

    [Fact]
    public void Return_AddsBackToPool()
    {
        var pool = new Pool<TestObj>(() => new TestObj());
        var obj = pool.Rent();
        pool.Return(obj);
        Assert.Equal(1, pool.AvailableCount);
        Assert.Equal(0, pool.RentedCount);
    }

    [Fact]
    public void Rent_ReusesReturnedObject()
    {
        var pool = new Pool<TestObj>(() => new TestObj());
        var a = pool.Rent();
        a.Value = 42;
        pool.Return(a);
        var b = pool.Rent();
        Assert.Same(a, b);
        Assert.Equal(42, b.Value); // значение сохранилось — onRent/onReturn должны чистить
    }

    [Fact]
    public void InitialSize_Prewarms()
    {
        var pool = new Pool<TestObj>(() => new TestObj(), initialSize: 5);
        Assert.Equal(5, pool.AvailableCount);
        Assert.Equal(5, pool.CreatedCount);
    }

    [Fact]
    public void MaxSize_LimitsPool()
    {
        var pool = new Pool<TestObj>(() => new TestObj(), maxSize: 3);
        var objs = new TestObj[5];
        for (int i = 0; i < 5; i++) objs[i] = pool.Rent();
        for (int i = 0; i < 5; i++) pool.Return(objs[i]);
        Assert.Equal(3, pool.AvailableCount); // только 3 вернулись в пул
    }

    [Fact]
    public void OnRent_And_OnReturn_Called()
    {
        int rentCount = 0;
        int returnCount = 0;
        var pool = new Pool<TestObj>(
            () => new TestObj(),
            onRent: _ => rentCount++,
            onReturn: _ => returnCount++);

        var obj = pool.Rent();
        pool.Return(obj);

        Assert.Equal(1, rentCount);
        Assert.Equal(1, returnCount);
    }

    [Fact]
    public void PoolManager_RegisterAndGet()
    {
        var pm = new PoolManager();
        var pool = pm.Create(() => new TestObj(), initialSize: 2);
        Assert.Equal(2, pm.Get<TestObj>().AvailableCount);
        Assert.True(pm.Has<TestObj>());
    }
}