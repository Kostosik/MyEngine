namespace MyEngine.Dialogue;

/// <summary>
/// Контекст диалога: флаги и значения, которые проверяют условия.
/// Игра наполняет его — например, "has_met_keeper" = true после первой встречи.
/// </summary>
public sealed class DialogueContext
{
    private readonly Dictionary<string, bool> _flags = new();

    public bool GetFlag(string name) => _flags.GetValueOrDefault(name, false);
    public void SetFlag(string name, bool value) => _flags[name] = value;
    public void ClearFlag(string name) => _flags[name] = false;

    public void Clear() => _flags.Clear();

    /// <summary>Разобрать строку "flag1,flag2,flag3" и установить все в true.</summary>
    public void SetFlagsFromList(string? list)
    {
        if (string.IsNullOrEmpty(list)) return;
        foreach (var f in list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            _flags[f] = true;
    }

    /// <summary>Разобрать строку "flag1,flag2" и снять все.</summary>
    public void ClearFlagsFromList(string? list)
    {
        if (string.IsNullOrEmpty(list)) return;
        foreach (var f in list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            _flags[f] = false;
    }
}