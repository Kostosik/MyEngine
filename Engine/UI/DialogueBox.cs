using ImGuiNET;
using MyEngine.Dialogue;
using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// ImGui-окно диалога. Связано с DialogueRunner — показывает текущий узел.
/// Ввод обрабатывается снаружи (в игре) — Box только отображает.
/// </summary>
public sealed class DialogueBox
{
    private DialogueRunner? _runner;

    public bool IsOpen => _runner?.IsRunning ?? false;
    public bool JustOpened { get; private set; }
    public DialogueRunner? Runner => _runner;

    /// <summary>Открыть с указанным runner'ом. Runner уже запущен.</summary>
    public void Open(DialogueRunner runner)
    {
        _runner = runner;
        JustOpened = true;
    }

    /// <summary>Закрыть — завершает runner и очищает состояние.</summary>
    public void Close()
    {
        _runner?.End();
        _runner = null;
        JustOpened = false;
    }

    public void ClearJustOpened() => JustOpened = false;

    public void Draw()
    {
        if (_runner == null || !_runner.IsRunning) return;

        var node = _runner.CurrentNode;
        if (node == null) return;

        var io = ImGui.GetIO();
        float boxW = io.DisplaySize.X * 0.7f;
        float boxH = node.HasChoices ? 240f : 130f;
        float x = (io.DisplaySize.X - boxW) * 0.5f;
        float y = io.DisplaySize.Y - boxH - 30f;

        ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(boxW, boxH), ImGuiCond.Always);

        ImGui.Begin("dialogue",
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoSavedSettings);

        // Спикер — либо из узла, либо из дерева
        string speaker = string.IsNullOrEmpty(node.Speaker)
            ? _runner.CurrentTree?.Speaker ?? ""
            : node.Speaker;

        ImGui.TextColored(new Vector4(0.7f, 0.85f, 1f, 1f), speaker);
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(node.Text);
        ImGui.Spacing();

        if (node.HasChoices)
        {
            // Кнопки вариантов
            var choices = _runner.AvailableChoices;
            for (int i = 0; i < choices.Count; i++)
            {
                string label = $"{i + 1}. {choices[i].Text}";
                if (ImGui.Button(label, new Vector2(-1, 28)))
                {
                    _runner.Choose(i);
                }
            }
        }
        else
        {
            ImGui.TextDisabled("[E] continue   [Esc] close");
        }

        ImGui.End();
    }
}