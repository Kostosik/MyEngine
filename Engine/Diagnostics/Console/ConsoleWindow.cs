using ImGuiNET;
using System.Numerics;

namespace MyEngine.Diagnostics.Console;

/// <summary>
/// ImGui-окно консоли. Открывается по клавише, отображается поверх всего.
/// Ввод: строка + Enter. История: стрелки Up/Down.
/// </summary>
public sealed class ConsoleWindow
{
    private readonly ConsoleSystem _console;
    private string _input = "";
    private bool _scrollToBottom = false;
    private bool _focusInput = false;
    private int _lastOutputCount;

    public bool Visible { get; set; } = false;

    public ConsoleWindow(ConsoleSystem console) => _console = console;

    public void Toggle() => Visible = !Visible;

    public void Show()
    {
        Visible = true;
        _focusInput = true;
        _scrollToBottom = true;
    }

    public void Hide() => Visible = false;

    public void Draw()
    {
        if (!Visible) return;

        var io = ImGui.GetIO();
        var size = new Vector2(io.DisplaySize.X * 0.7f, io.DisplaySize.Y * 0.5f);
        ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);

        ImGui.Begin("console",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus);

        // --- Лог ---
        float footerHeight = ImGui.GetFrameHeightWithSpacing() + 8;
        ImGui.BeginChild("log", new Vector2(0, -footerHeight), ImGuiChildFlags.None);

        var output = _console.Output;
        for (int i = 0; i < output.Count; i++)
        {
            var line = output[i];
            if (line.StartsWith("[ERR]"))
                ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), line);
            else if (line.StartsWith("[INF]"))
                ImGui.TextColored(new Vector4(0.7f, 0.9f, 1f, 1f), line);
            else
                ImGui.Text(line);
        }

        // Автоскролл при новых строках
        if (_lastOutputCount != output.Count)
        {
            _lastOutputCount = output.Count;
            _scrollToBottom = true;
        }
        if (_scrollToBottom)
        {
            ImGui.SetScrollHereY(1.0f);
            _scrollToBottom = false;
        }

        ImGui.EndChild();

        // --- Ввод ---
        if (_focusInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusInput = false;
        }

        ImGui.PushItemWidth(-1);
        var flags = ImGuiInputTextFlags.EnterReturnsTrue;
        bool submitted = ImGui.InputText("##input", ref _input, 512, flags);

        if (submitted)
        {
            _console.Execute(_input);
            _input = "";
            _focusInput = true;
            _scrollToBottom = true;
        }

        ImGui.PopItemWidth();

        ImGui.End();
    }
}