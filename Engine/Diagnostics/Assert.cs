using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MyEngine.Diagnostics;

/// <summary>
/// Assertions для отладки. В Debug-сборке падают с сообщением при ошибке,
/// в Release — полностью исчезают (компилятор вырезает вызовы благодаря
/// [Conditional("DEBUG")]).
///
/// Использование:
///   Assert.NotNull(entity, "Player is null");
///   Assert.True(hp >= 0, $"HP = {hp}");
///   Assert.InRange(speed, 0f, 1000f);
/// </summary>
public static class Assert
{
    [Conditional("DEBUG")]
    public static void True(bool condition, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (condition) return;
        Fail($"Assertion failed: expected true", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void False(bool condition, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (!condition) return;
        Fail("Assertion failed: expected false", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void NotNull(object? obj, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (obj != null) return;
        Fail("Assertion failed: expected not null", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void Null(object? obj, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (obj == null) return;
        Fail("Assertion failed: expected null", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void Equal<T>(T expected, T actual, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual)) return;
        Fail($"Assertion failed: expected {expected}, got {actual}", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void InRange(float value, float min, float max, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (value >= min && value <= max) return;
        Fail($"Assertion failed: {value} not in [{min}, {max}]", message, file, line);
    }

    [Conditional("DEBUG")]
    public static void NotEmpty<T>(IReadOnlyCollection<T> collection, string? message = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (collection.Count > 0) return;
        Fail("Assertion failed: collection is empty", message, file, line);
    }

    private static void Fail(string baseMsg, string? userMsg, string file, int line)
    {
        var msg = userMsg != null ? $"{baseMsg}\n{userMsg}" : baseMsg;
        var location = $"{Path.GetFileName(file)}:{line}";

        // Лог
        Log.Error("Assert", $"{msg}\n  at {location}");

        // Бросаем — в debug хотим упасть, а не тихо продолжать
        throw new AssertionException($"{msg} (at {location})");
    }
}

/// <summary>Исключение, которое бросает Assert. Отличается от обычного Exception, чтобы можно было отфильтровать.</summary>
public sealed class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}