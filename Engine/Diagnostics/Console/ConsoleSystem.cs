using System.Text;

namespace MyEngine.Diagnostics.Console;

/// <summary>
/// Ядро консоли: регистрация команд, история, вывод.
/// Не привязана к UI — ImGui-окно подключается отдельно.
/// </summary>
public sealed class ConsoleSystem
{
    private readonly Dictionary<string, IConsoleCommand> _commands = new();
    private readonly List<string> _history = new();      // введённые команды
    private readonly List<string> _output = new();       // строки вывода
    private readonly int _maxOutputLines;
    private readonly int _maxHistory;
    private int _historyIndex = -1;

    public IReadOnlyList<string> Output => _output;
    public IReadOnlyList<string> History => _history;
    public IEnumerable<IConsoleCommand> Commands => _commands.Values;

    public ConsoleSystem(int maxOutputLines = 256, int maxHistory = 64)
    {
        _maxOutputLines = maxOutputLines;
        _maxHistory = maxHistory;
    }

    // ============================================================
    // Регистрация
    // ============================================================

    public void Register(IConsoleCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Command name must not be empty");

        if (_commands.ContainsKey(command.Name))
            Log.Warn("Console", $"Command '{command.Name}' already registered, overwriting");

        _commands[command.Name] = command;
    }

    public void Register(string name, string description, string argsHint,
        Func<string[], string> handler)
    {
        Register(new LambdaCommand(name, description, argsHint, handler));
    }

    public void Unregister(string name) => _commands.Remove(name);
    public void ClearCommands() => _commands.Clear();

    // ============================================================
    // Вывод
    // ============================================================

    /// <summary>Написать в консоль.</summary>
    public void Write(string line)
    {
        _output.Add(line);
        if (_output.Count > _maxOutputLines)
            _output.RemoveRange(0, _output.Count - _maxOutputLines);
    }

    public void WriteError(string line) => Write($"[ERR] {line}");
    public void WriteInfo(string line) => Write($"[INF] {line}");

    public void Clear() => _output.Clear();

    // ============================================================
    // Выполнение
    // ============================================================

    /// <summary>
    /// Выполнить строку. Разбирает на команду и аргументы.
    /// Поддерживает кавычки для аргументов с пробелами: say "hello world".
    /// </summary>
    public string Execute(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        // История
        _history.Add(input);
        if (_history.Count > _maxHistory) _history.RemoveAt(0);
        _historyIndex = -1;

        // Парсинг
        var parts = ParseArgs(input);
        if (parts.Length == 0) return "";

        var name = parts[0];
        var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (!_commands.TryGetValue(name, out var cmd))
        {
            var msg = $"Unknown command: {name}. Try 'help'.";
            WriteError(msg);
            return msg;
        }

        try
        {
            var result = cmd.Execute(args) ?? "";
            if (!string.IsNullOrEmpty(result))
                Write(result);
            return result;
        }
        catch (Exception ex)
        {
            var msg = $"Command failed: {ex.Message}";
            WriteError(msg);
            Log.Error("Console", $"Command '{name}' threw", ex);
            return msg;
        }
    }

    /// <summary>История вверх (стрелка Up).</summary>
    public string? HistoryUp()
    {
        if (_history.Count == 0) return null;
        if (_historyIndex == -1) _historyIndex = _history.Count;
        _historyIndex = System.Math.Max(0, _historyIndex - 1);
        return _history[_historyIndex];
    }

    /// <summary>История вниз (стрелка Down).</summary>
    public string? HistoryDown()
    {
        if (_history.Count == 0 || _historyIndex == -1) return null;
        _historyIndex = System.Math.Min(_history.Count, _historyIndex + 1);
        if (_historyIndex >= _history.Count) return "";
        return _history[_historyIndex];
    }

    /// <summary>Автодополнение по началу имени.</summary>
    public IEnumerable<string> Complete(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) yield break;
        foreach (var name in _commands.Keys)
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                yield return name;
    }

    // ============================================================
    // Парсинг
    // ============================================================

    private static string[] ParseArgs(string input)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (sb.Length > 0)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0) result.Add(sb.ToString());
        return result.ToArray();
    }
}