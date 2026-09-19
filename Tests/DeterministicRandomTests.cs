using MyEngine.Utility;

namespace Tests;

public class DeterministicRandomTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var r1 = new DeterministicRandom(42);
        var r2 = new DeterministicRandom(42);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(r1.NextInt(1000), r2.NextInt(1000));
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSequence()
    {
        var r1 = new DeterministicRandom(42);
        var r2 = new DeterministicRandom(43);

        int sameCount = 0;
        for (int i = 0; i < 100; i++)
        {
            if (r1.NextInt(1000) == r2.NextInt(1000)) sameCount++;
        }

        Assert.True(sameCount < 20, "Разные seed должны давать разные последовательности");
    }

    [Fact]
    public void NextInt_InRange()
    {
        var r = new DeterministicRandom(1);
        for (int i = 0; i < 1000; i++)
        {
            int v = r.NextInt(10, 20);
            Assert.InRange(v, 10, 19);
        }
    }

    [Fact]
    public void NextFloat_InRange()
    {
        var r = new DeterministicRandom(1);
        for (int i = 0; i < 1000; i++)
        {
            float v = r.NextFloat();
            Assert.InRange(v, 0f, 1f);
        }
    }

    [Fact]
    public void SaveAndLoad_RestoresState()
    {
        var r1 = new DeterministicRandom(42);
        for (int i = 0; i < 50; i++) r1.NextInt(100);

        var (s0, s1) = r1.SaveState();

        // Проматываем ещё
        for (int i = 0; i < 50; i++) r1.NextInt(100);
        int expected = r1.NextInt(100);

        // Восстанавливаем — получим то же
        var r2 = new DeterministicRandom(1);
        r2.LoadState(s0, s1);
        for (int i = 0; i < 50; i++) r2.NextInt(100);
        int actual = r2.NextInt(100);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Shuffle_Deterministic()
    {
        var r1 = new DeterministicRandom(42);
        var r2 = new DeterministicRandom(42);

        var a = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var b = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        r1.Shuffle(a);
        r2.Shuffle(b);

        Assert.Equal(a, b);
    }
}