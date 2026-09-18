namespace MyEngine.Diagnostics.Validation;

/// <summary>Поле должно быть > 0.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PositiveAttribute : Attribute { }