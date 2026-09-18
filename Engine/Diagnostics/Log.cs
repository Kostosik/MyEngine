using System.Text;

namespace MyEngine.Diagnostics;

public enum LogLevel
{
    Trace,   // очень подробно — для отладки конкретной системы
    Debug,   // отладочное — что происходит
    Info,    // обычное — важные события
    Warn,    // предупреждения — что-то подозрительное
    Error,   // ошибки — что-то сломалось
    None     // выключить полностью
}

/// <summary>
/// Централизованный лог с уровнями и категориями.
///
/// Использование:
///   Log.Info("AI", "Enemy spotted player");
///   Log.Warn("Physics", "Body stuck in wall");
///   Log.Error("Audio", "Failed to load sound", ex);
///
/// Фильтры:
///   Log.MinLevel = LogLevel.Info;   — не показывать Trace/Debug
///   Log.SetCategoryLevel("AI", LogLevel.Debug); — для AI подробнее
/// </summary>
public static class Log
{
    private static readonly object _lock = new();
    private static string _logPath = "logs/game.log";
    private static bool _initialized;

    /// <summary>Минимальный уровень для вывода. Всё ниже игнорируется.</summary>
    public static LogLevel MinLevel = LogLevel.Info;

    /// <summary>Если true — писать в консоль. Если false — только в файл.</summary>
    public static bool WriteToConsole = true;

    /// <summary>Если true — писать в файл. В Release можно выключить.</summary>
    public static bool WriteToFile = true;

    /// <summary>Категории, для которых уровень повышен/понижен.</summary>
    private static readonly Dictionary<string, LogLevel> _categoryLevels = new();

    public static void Initialize(string logPath)
    {
        _logPath = logPath;
        try
        {
            var dir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(logPath, $"=== Log started {DateTime.Now:O} ===\n");
        }
        catch { }
        _initialized = true;
    }

    public static void SetCategoryLevel(string category, LogLevel level)
        => _categoryLevels[category] = level;

    public static void ClearCategoryLevels() => _categoryLevels.Clear();

    // ============ Основные методы ============

    public static void Trace(string category, string message) => Write(LogLevel.Trace, category, message);
    public static void Debug(string category, string message) => Write(LogLevel.Debug, category, message);
    public static void Info(string category, string message) => Write(LogLevel.Info, category, message);
    public static void Warn(string category, string message) => Write(LogLevel.Warn, category, message);

    public static void Error(string category, string message) => Write(LogLevel.Error, category, message);
    public static void Error(string category, string message, Exception ex)
        => Write(LogLevel.Error, category, $"{message}\n{ex}");

    // ============ Ядро ============

    private static void Write(LogLevel level, string category, string message)
    {
        // Проверка уровня для категории
        var minForCategory = _categoryLevels.GetValueOrDefault(category, MinLevel);
        if (level < minForCategory) return;

        var line = FormatLine(level, category, message);

        if (WriteToConsole) WriteConsole(level, line);
        if (WriteToFile && _initialized) WriteFile(line);
    }

    private static string FormatLine(LogLevel level, string category, string message)
    {
        string levelStr = level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warn => "WRN",
            LogLevel.Error => "ERR",
            _ => "???"
        };
        return $"[{DateTime.Now:HH:mm:ss.fff}] [{levelStr}] [{category}] {message}";
    }

    private static void WriteConsole(LogLevel level, string line)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = level switch
        {
            LogLevel.Trace => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => prev
        };
        System.Console.WriteLine(line);
        System.Console.ForegroundColor = prev;
    }

    private static void WriteFile(string line)
    {
        lock (_lock)
        {
            try { File.AppendAllText(_logPath, line + Environment.NewLine); }
            catch { }
        }
    }
}