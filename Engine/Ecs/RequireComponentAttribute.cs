namespace MyEngine.Ecs;

/// <summary>
/// Объявляет, что компонент требует наличия других компонентов.
/// Проверяется при Add в Debug.
///
/// Использование:
///   [RequireComponent(typeof(Transform))]
///   public sealed class TopDownController { ... }
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequireComponentAttribute : Attribute
{
    public Type[] RequiredTypes { get; }

    public RequireComponentAttribute(params Type[] types)
    {
        RequiredTypes = types;
    }
}