namespace MyEngine.Diagnostics;

/// <summary>
/// Парсер командной строки. Хранит флаги (--fullscreen) и пары
/// ключ-значение (--level=forest, --width 1280).
///
/// Пример запуска:
///   MyGame.exe --fullscreen --level=forest --width 1920 --debug
///
/// Использование:
///   CommandLine.Initialize(args);
///   if (CommandLine.HasFlag("fullscreen")) { ... }
///   string level = CommandLine.GetString("level", "island");
///   int width = CommandLine.GetInt("width", 1280);
/// </summary>
public static class CommandLine
{
    private static readonly Dictionary<string, string> _values = new();
    private static readonly HashSet<string> _flags = new();

    /// <summary>Сырой массив аргументов (для отладки).</summary>
    public static IReadOnlyList<string> RawArgs { get; private set; } = Array.Empty<string>();

    public static void Initialize(string[] args)
    {
        _values.Clear();
        _flags.Clear();
        RawArgs = args;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.IsNullOrWhiteSpace(arg)) continue;

            // Убираем ведущие -- или -
            if (arg.StartsWith("--")) arg = arg[2..];
            else if (arg.StartsWith("-")) arg = arg[1..];

            // Формат --key=value
            int eq = arg.IndexOf('=');
            if (eq >= 0)
            {
                var key = arg[..eq].Trim().ToLowerInvariant();
                var value = arg[(eq + 1)..];
                _values[key] = value;
                continue;
            }

            // Формат --key value (значение в следующем аргументе)
            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                var key = arg.ToLowerInvariant();
                var value = args[i + 1];
                _values[key] = value;
                i++; // пропускаем следующий
                continue;
            }

            // Иначе — просто флаг
            _flags.Add(arg.ToLowerInvariant());
        }

        if (args.Length > 0)
            Log.Info("CommandLine", $"Args: {string.Join(" ", args)}");
    }

    // ============================================================
    // Запросы
    // ============================================================

    /// <summary>Есть ли флаг. --fullscreen → HasFlag("fullscreen") = true.</summary>
    public static bool HasFlag(string name)
        => _flags.Contains(name.ToLowerInvariant());

    /// <summary>Была ли задана опция (флаг или ключ).</summary>
    public static bool Has(string name)
    {
        var key = name.ToLowerInvariant();
        return _flags.Contains(key) || _values.ContainsKey(key);
    }

    /// <summary>Строковое значение. Если нет — default.</summary>
    public static string GetString(string name, string defaultValue = "")
        => _values.GetValueOrDefault(name.ToLowerInvariant(), defaultValue);

    /// <summary>Целое значение. Если нет или не парсится — default.</summary>
    public static int GetInt(string name, int defaultValue = 0)
    {
        var s = GetString(name);
        return int.TryParse(s, out int v) ? v : defaultValue;
    }

    /// <summary>Дробное значение.</summary>
    public static float GetFloat(string name, float defaultValue = 0f)
    {
        var s = GetString(name);
        return float.TryParse(s, out float v) ? v : defaultValue;
    }

    /// <summary>Булево значение. true/false/1/0/yes/no.</summary>
    public static bool GetBool(string name, bool defaultValue = false)
    {
        var s = GetString(name).ToLowerInvariant();
        if (string.IsNullOrEmpty(s)) return HasFlag(name) ? true : defaultValue;
        return s switch
        {
            "true" or "1" or "yes" or "on" => true,
            "false" or "0" or "no" or "off" => false,
            _ => defaultValue
        };
    }

    /// <summary>Все значения (для отладки).</summary>
    public static IReadOnlyDictionary<string, string> AllValues => _values;
    public static IReadOnlyCollection<string> AllFlags => _flags;
}