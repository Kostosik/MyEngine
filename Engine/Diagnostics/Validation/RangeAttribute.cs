namespace MyEngine.Diagnostics.Validation;

/// <summary>
/// Поле или свойство должно быть в диапазоне [Min, Max].
/// Работает и с int, и с float.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RangeAttribute : Attribute
{
    public double Min { get; }
    public double Max { get; }

    public RangeAttribute(double min, double max)
    {
        if (min > max)
            throw new ArgumentException("Min must be <= Max");
        Min = min;
        Max = max;
    }
}