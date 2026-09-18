using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MyEngine.Diagnostics;

/// <summary>
/// Проверки аргументов и инвариантов. Работает как Assert —
/// в Debug бросает исключение, в Release стирается компилятором.
///
/// Отличие от Assert: Guard специально для проверки ВХОДНЫХ
/// параметров методов и бросков с информативным именем аргумента.
///
/// Использование:
///   public void Update(World world, float dt)
///   {
///       Guard.NotNull(world);
///       Guard.Positive(dt, nameof(dt));
///       Guard.InRange(count, 0, 1000);
///   }
/// </summary>
public static class Guard
{
    // ============================================================
    // Null / not null
    // ============================================================

    [Conditional("DEBUG")]
    public static void NotNull(object? value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value == null)
            Throw($"Argument '{name}' must not be null", name);
    }

    [Conditional("DEBUG")]
    public static void Null(object? value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value != null)
            Throw($"Argument '{name}' must be null", name);
    }

    // ============================================================
    // Числа
    // ============================================================

    [Conditional("DEBUG")]
    public static void Positive(int value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value <= 0)
            Throw($"Argument '{name}' must be positive, got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void Positive(float value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value <= 0f)
            Throw($"Argument '{name}' must be positive, got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void NonNegative(int value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value < 0)
            Throw($"Argument '{name}' must be non-negative, got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void NonNegative(float value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value < 0f)
            Throw($"Argument '{name}' must be non-negative, got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void InRange(int value, int min, int max,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value < min || value > max)
            Throw($"Argument '{name}' must be in [{min}, {max}], got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void InRange(float value, float min, float max,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value < min || value > max)
            Throw($"Argument '{name}' must be in [{min}, {max}], got {value}", name);
    }

    [Conditional("DEBUG")]
    public static void InRange(double value, double min, double max,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (value < min || value > max)
            Throw($"Argument '{name}' must be in [{min}, {max}], got {value}", name);
    }

    // ============================================================
    // Строки
    // ============================================================

    [Conditional("DEBUG")]
    public static void NotNullOrEmpty(string? value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (string.IsNullOrEmpty(value))
            Throw($"Argument '{name}' must not be null or empty", name);
    }

    [Conditional("DEBUG")]
    public static void NotNullOrWhiteSpace(string? value,
        [CallerArgumentExpression("value")] string? name = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            Throw($"Argument '{name}' must not be null or whitespace", name);
    }

    // ============================================================
    // Коллекции
    // ============================================================

    [Conditional("DEBUG")]
    public static void NotEmpty<T>(IReadOnlyCollection<T>? collection,
        [CallerArgumentExpression("collection")] string? name = null)
    {
        if (collection == null || collection.Count == 0)
            Throw($"Argument '{name}' must not be empty", name);
    }

    // ============================================================
    // Логическое
    // ============================================================

    [Conditional("DEBUG")]
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
            Throw(message ?? "Condition must be true", null);
    }

    [Conditional("DEBUG")]
    public static void False(bool condition, string? message = null)
    {
        if (condition)
            Throw(message ?? "Condition must be false", null);
    }

    // ============================================================
    // Enum
    // ============================================================

    [Conditional("DEBUG")]
    public static void EnumDefined<T>(T value,
        [CallerArgumentExpression("value")] string? name = null) where T : struct, Enum
    {
        if (!System.Enum.IsDefined(value))
            Throw($"Argument '{name}' is not a defined enum value: {value}", name);
    }

    // ============================================================
    // Внутреннее
    // ============================================================

    [DebuggerHidden]
    [DebuggerStepThrough]
    private static void Throw(string message, string? paramName)
    {
        Log.Error("Guard", message);
        if (paramName != null)
            throw new ArgumentException(message, paramName);
        throw new InvalidOperationException(message);
    }
}