using ImGuiNET;
using System.Numerics;

namespace MyEngine.Diagnostics;

/// <summary>
/// Экран фатальной ошибки. Рисуется, когда основная логика упала.
/// Не даёт игре тихо закрыться — показывает стек и путь к логу.
///
/// Использование в Application:
///   _fatalError.Capture(ex);
///   // ... в OnFrame:
///   if (_fatalError.HasError) _fatalError.Draw();
/// </summary>
public sealed class FatalErrorOverlay
{
    private bool _hasError;
    private string _message = "";

    public bool HasError => _hasError;
    public string Message => _message;

    /// <summary>Зафиксировать ошибку. Первая ошибка — важная, остальные игнорируются.</summary>
    public void Capture(Exception ex, string source = "")
    {
        if (_hasError) return;   // не перезаписываем первую

        _hasError = true;
        var header = string.IsNullOrEmpty(source) ? "" : $"[{source}] ";
        _message = $"{header}{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}";

        ErrorLogger.Log(source, ex);
    }

    /// <summary>Проверить и зафиксировать ошибку из блока. Возвращает true, если была ошибка.</summary>
    public bool TryCatch(Action action, string source)
    {
        if (_hasError) return true;
        try
        {
            action();
            return false;
        }
        catch (Exception ex)
        {
            Capture(ex, source);
            return true;
        }
    }

    /// <summary>Сбросить. Не сбрасываем — после фатала игру не продолжаем.</summary>
    public void Reset()
    {
        _hasError = false;
        _message = "";
    }

    /// <summary>Нарисовать экран ошибки. Безопасно вызывать в любом состоянии.</summary>
    public void Draw()
    {
        if (!_hasError) return;

        try
        {
            var io = ImGui.GetIO();
            ImGui.SetNextWindowPos(new Vector2(20, 20), ImGuiCond.Always);
            ImGui.SetNextWindowSize(
                new Vector2(io.DisplaySize.X - 40, io.DisplaySize.Y - 40),
                ImGuiCond.Always);

            ImGui.Begin("FATAL ERROR",
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);

            ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), "Engine crashed. See log for details.");
            ImGui.Separator();
            ImGui.TextWrapped(_message);
            ImGui.Separator();
            ImGui.Text($"Log: {Path.GetFullPath("logs/errors.log")}");

            ImGui.End();
        }
        catch
        {
            // Не можем нарисовать — не повод падать второй раз
        }
    }
}