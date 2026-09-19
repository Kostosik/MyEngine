using MyEngine.Diagnostics.Validation;

namespace Tests;

public class ComponentValidatorTests
{
    private class ValidComponent
    {
        [Range(0, 100)]
        public int Hp = 50;

        [Positive]
        public int MaxHp = 100;

        [NotNull]
        public string Name = "ok";

        [NotEmpty]
        public string Title = "hello";

        [NotEmpty]
        public List<int> Items = new() { 1, 2, 3 };
    }

    private class InvalidHpRange
    {
        [Range(0, 100)]
        public int Hp = 150;
    }

    private class InvalidMaxHpPositive
    {
        [Positive]
        public int MaxHp = 0;
    }

    private class InvalidNotNull
    {
        [NotNull]
        public string? Name = null;
    }

    private class InvalidNotEmptyString
    {
        [NotEmpty]
        public string Title = "";
    }

    private class InvalidNotEmptyList
    {
        [NotEmpty]
        public List<int> Items = new();
    }

    [Fact]
    public void Valid_Passes()
    {
        ComponentValidator.Validate(new ValidComponent());
    }

    [Fact]
    public void Range_ThrowsAbove()
    {
        Assert.Throws<InvalidOperationException>(
            () => ComponentValidator.Validate(new InvalidHpRange()));
    }

    [Fact]
    public void Positive_ThrowsOnZero()
    {
        Assert.Throws<InvalidOperationException>(
            () => ComponentValidator.Validate(new InvalidMaxHpPositive()));
    }

    [Fact]
    public void NotNull_ThrowsOnNull()
    {
        Assert.Throws<InvalidOperationException>(
            () => ComponentValidator.Validate(new InvalidNotNull()));
    }

    [Fact]
    public void NotEmpty_ThrowsOnEmptyString()
    {
        Assert.Throws<InvalidOperationException>(
            () => ComponentValidator.Validate(new InvalidNotEmptyString()));
    }

    [Fact]
    public void NotEmpty_ThrowsOnEmptyList()
    {
        Assert.Throws<InvalidOperationException>(
            () => ComponentValidator.Validate(new InvalidNotEmptyList()));
    }
}