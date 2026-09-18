namespace MyEngine.Diagnostics.Validation;

/// <summary>Строка или коллекция не должна быть пустой.</summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NotEmptyAttribute : Attribute { }