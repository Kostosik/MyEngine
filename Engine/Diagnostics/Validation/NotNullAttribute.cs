namespace MyEngine.Diagnostics.Validation;

/// <summary>Ссылочное поле не должно быть null.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NotNullAttribute : Attribute { }