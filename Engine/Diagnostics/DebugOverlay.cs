using MyEngine.InputEngine;

namespace MyEngine.Diagnostics;

/// <summary>
/// Реестр отладочных инструментов. Сам ничего не создаёт,
/// только хранит и вызывает. Application (или игра) сама
/// регистрирует то, что нужно.
///
/// Пример:
///   _debug = new DebugOverlay();
///   _debug.Register(new ConsoleTool());
///   _debug.Register(new WatchTool());
///   // AssetBrowser не регистрируем — не нужен
/// </summary>
public sealed class DebugOverlay
{
    private readonly List<IDebugTool> _tools = new();

    public IReadOnlyList<IDebugTool> Tools => _tools;

    /// <summary>Зарегистрировать инструмент.</summary>
    public void Register(IDebugTool tool) => _tools.Add(tool);

    /// <summary>Получить инструмент по типу.</summary>
    public T? Get<T>() where T : class, IDebugTool
    {
        foreach (var t in _tools)
            if (t is T typed) return typed;
        return null;
    }

    /// <summary>Все F-клавиши обрабатываются тут.</summary>
    public void ProcessHotkeys(Input input)
    {
        foreach (var tool in _tools)
            tool.ProcessHotkeys(input);
    }

    /// <summary>Все видимые окна рисуются тут.</summary>
    public void Draw()
    {
        foreach (var tool in _tools)
            if (tool.Visible) tool.Draw();
    }

    /// <summary>Есть ли открытое окно, блокирующее игру (например, консоль).</summary>
    public bool BlocksGameInput
    {
        get
        {
            foreach (var tool in _tools)
                if (tool is IBlockingTool b && b.BlocksGameInput) return true;
            return false;
        }
    }

    public void Dispose()
    {
        foreach (var tool in _tools)
            if (tool is IDisposable d) d.Dispose();
        _tools.Clear();
    }
}

/// <summary>Инструмент, который может блокировать игру (например, консоль).</summary>
public interface IBlockingTool
{
    bool BlocksGameInput { get; }
}