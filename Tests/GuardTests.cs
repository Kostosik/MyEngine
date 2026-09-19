using MyEngine.Diagnostics;

namespace Tests;

public class GuardTests
{
    [Fact]
    public void NotNull_PassesOnValue()
    {
        var obj = new object();
        Guard.NotNull(obj); // не должно бросить
    }

    [Fact]
    public void NotNull_ThrowsOnNull()
    {
        object? obj = null;
        Assert.Throws<ArgumentException>(() => Guard.NotNull(obj));
    }

    [Fact]
    public void Positive_ThrowsOnZero()
    {
        Assert.Throws<ArgumentException>(() => Guard.Positive(0));
    }

    [Fact]
    public void Positive_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => Guard.Positive(-5));
    }

    [Fact]
    public void Positive_PassesOnPositive()
    {
        Guard.Positive(10);
    }

    [Fact]
    public void InRange_ThrowsBelow()
    {
        Assert.Throws<ArgumentException>(() => Guard.InRange(-1, 0, 100));
    }

    [Fact]
    public void InRange_ThrowsAbove()
    {
        Assert.Throws<ArgumentException>(() => Guard.InRange(101, 0, 100));
    }

    [Fact]
    public void InRange_PassesInside()
    {
        Guard.InRange(50, 0, 100);
    }

    [Fact]
    public void NotNullOrEmpty_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(""));
    }

    [Fact]
    public void NotNullOrEmpty_PassesOnText()
    {
        Guard.NotNullOrEmpty("hello");
    }

    [Fact]
    public void NotEmpty_ThrowsOnEmptyList()
    {
        var list = new List<int>();
        Assert.Throws<ArgumentException>(() => Guard.NotEmpty(list));
    }

    [Fact]
    public void EnumDefined_ThrowsOnInvalid()
    {
        Assert.Throws<ArgumentException>(() => Guard.EnumDefined((DayOfWeek)999));
    }

    [Fact]
    public void EnumDefined_PassesOnValid()
    {
        Guard.EnumDefined(DayOfWeek.Monday);
    }
}