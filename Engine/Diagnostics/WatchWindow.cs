using ImGuiNET;
using System.Numerics;

namespace MyEngine.Diagnostics;

/// <summary>
/// ImGui-окно со списком Watch-значений.
/// Открывается по клавише, обновляется каждый кадр.
/// Значения лямбд вычисляются ТОЛЬКО когда окно видимо.
/// </summary>
public sealed class WatchWindow
{
    public bool Visible { get; set; } = false;

    /// <summary>Автообновление значений. Если false — пауза между обновлениями.</summary>
    public bool AutoUpdate { get; set; } = true;

    private float _updateTimer;
    private float _updateInterval = 0.1f;   // обновлять 10 раз в секунду, а не каждый кадр

    private readonly List<(string name, string value)> _cached = new();

    public void Toggle() => Visible = !Visible;

    public void Draw()
    {
        if (!Visible) return;

        ImGui.SetNextWindowSize(new Vector2(320, 400), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(
            new Vector2(ImGui.GetIO().DisplaySize.X - 680, 360),
            ImGuiCond.FirstUseEver);

        ImGui.Begin("watch",
            ImGuiWindowFlags.NoSavedSettings);

        // === Тулбар ===
        if (ImGui.SmallButton("Refresh")) RefreshAll();
        ImGui.SameLine();
        bool auto = AutoUpdate;
        ImGui.Checkbox("Auto", ref auto);
        AutoUpdate = auto;

        if (AutoUpdate)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            ImGui.DragFloat("##interval", ref _updateInterval,
                0.01f, 0.02f, 2f, "%.2fs");
        }
        else
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(manual)");
        }

        ImGui.SameLine();
        if (ImGui.SmallButton("Clear")) Watch.Clear();

        ImGui.Separator();

        // === Автообновление ===
        if (AutoUpdate)
        {
            _updateTimer += ImGui.GetIO().DeltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                RefreshAll();
            }
        }

        // === Список ===
        if (_cached.Count == 0)
        {
            ImGui.TextDisabled("Нет значений. Вызови Watch.Add(...) в игре.");
            ImGui.End();
            return;
        }

        ImGui.BeginChild("list");

        foreach (var (name, value) in _cached)
        {
            if (string.IsNullOrEmpty(name))
            {
                // Пустая строка = разделитель между группами
                ImGui.Separator();
                continue;
            }

            if (name.StartsWith("== "))   // заголовок секции
            {
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(0.9f, 0.8f, 0.4f, 1f), name);
                continue;
            }

            ImGui.Text(name);
            ImGui.SameLine();
            ImGui.SetCursorPosX(200);
            ImGui.TextColored(new Vector4(0.7f, 0.9f, 1f, 1f), value);
        }

        ImGui.EndChild();
        ImGui.End();
    }

    /// <summary>Обновить все значения разом.</summary>
    private void RefreshAll()
    {
        _cached.Clear();

        var entries = Watch.Entries;
        foreach (var entry in entries)
        {
            if (entry.IsSection)
            {
                _cached.Add(($"== {entry.Name}", ""));
                continue;
            }

            string value;
            try
            {
                value = entry.Getter?.Invoke() ?? "(null)";
            }
            catch (Exception ex)
            {
                value = $"ERR: {ex.Message}";
            }

            _cached.Add((entry.Name, value));
        }
    }
}