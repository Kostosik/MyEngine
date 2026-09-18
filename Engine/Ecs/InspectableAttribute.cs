namespace MyEngine.Ecs;

/// <summary>
/// Пометить компонент, который должен отображаться в EntityInspector.
/// Если ни у одного компонента нет атрибута — показываются все.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class InspectableAttribute : Attribute { }