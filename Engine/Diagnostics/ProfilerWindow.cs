using ImGuiNET;
using System.Numerics;

namespace MyEngine.Diagnostics;

public sealed class ProfilerWindow
{
    private readonly Profiler _profiler;
    private bool _sortByAvg = true;


    public bool Visible { get; set; } = false;

    public ProfilerWindow(Profiler profiler) => _profiler = profiler;

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
    public void Draw()
    {
        if (!Visible) return;

        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 360, 20),
            ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(340, 400), ImGuiCond.Always);

        ImGui.Begin("profiler",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);

        // === Заголовок с бюджетом ===
        double budgetUse = _profiler.BudgetUsed;
        double avgUse = _profiler.AvgBudgetUsed;

        var headerColor = budgetUse > 1.0
            ? new Vector4(1f, 0.4f, 0.4f, 1f)
            : budgetUse > 0.8
                ? new Vector4(1f, 0.9f, 0.4f, 1f)
                : new Vector4(0.6f, 1f, 0.6f, 1f);

        ImGui.Text($"FPS: {_profiler.Fps:F0}   Frame: {_profiler.CurrentFrameMs:F2} ms");
        ImGui.Text($"Avg: {_profiler.AvgFrameMs:F2}   Peak: {_profiler.PeakFrameMs:F2} ms");

        ImGui.TextColored(headerColor,
            $"Budget: {_profiler.FrameBudgetMs:F2} ms   Used: {budgetUse * 100:F0}%");

        ImGui.Text($"Avg use: {avgUse * 100:F0}%   Over: {_profiler.TotalOverBudgetFrames}/{_profiler.TotalFrames}");

        // Progress bar
        var barColor = budgetUse > 1.0
            ? new Vector4(0.9f, 0.2f, 0.2f, 1f)
            : budgetUse > 0.8
                ? new Vector4(0.9f, 0.7f, 0.2f, 1f)
                : new Vector4(0.3f, 0.8f, 0.3f, 1f);

        // Прогрессбар — по сглаженному avg, а не по текущему кадру
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, barColor);
        ImGui.ProgressBar(
            (float)System.Math.Min(avgUse, 1.5),
            new Vector2(-1, 12),
            $"{avgUse * 100:F0}%");
        ImGui.PopStyleColor();

        // === График ===
        // PlotLines от ImGui — рисует из массива. Передаём историю как есть,
        // смещение = текущий индекс (чтобы "новое" было справа).
        ImGui.PlotLines("##frame",
            ref _profiler.FrameHistory[0],
            _profiler.FrameHistory.Length,
            _profiler.FrameHistoryIndex,
            null, 0f, 40f,
            new Vector2(-1, 70));

        ImGui.Separator();

        // === Кнопки ===
        if (ImGui.SmallButton("Reset")) _profiler.ResetStats();
        ImGui.SameLine();
        ImGui.Checkbox("Sort by avg", ref _sortByAvg);

        ImGui.Separator();

        // === Список систем ===
        var snap = _profiler.Snapshot();
        if (snap.Count == 0)
        {
            ImGui.TextDisabled("No systems measured yet.");
            ImGui.End();
            return;
        }

        // Сортировка
        var list = new List<KeyValuePair<string, (double last, double avg, double peak)>>(snap);
        if (_sortByAvg)
            list.Sort((a, b) => b.Value.avg.CompareTo(a.Value.avg));
        else
            list.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

        ImGui.Separator();

        // === GC ===
        if (ImGui.CollapsingHeader("GC / Memory", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text($"Managed: {FormatBytes(_profiler.ManagedMemoryBytes)}");

            // Аллокации за кадр — красный если больше 10 КБ
            var allocColor = _profiler.AllocatedPerFrameBytes > 10_000
                ? new Vector4(1f, 0.5f, 0.4f, 1f)
                : new Vector4(0.8f, 0.8f, 0.8f, 1f);
            ImGui.TextColored(allocColor,
                $"Alloc/frame: {FormatBytes(_profiler.AllocatedPerFrameBytes)}");

            ImGui.Text($"GC: G0={_profiler.GcGen0} G1={_profiler.GcGen1} G2={_profiler.GcGen2}");

            // Отслеживание: сколько сборок за интервал 0.5 сек
            // (упрощённо — показываем только общее число)
        }

        ImGui.Separator();

        // Таблица
        if (ImGui.BeginTable("systems", 4,
            ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("System", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Last", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Avg", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Peak", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableHeadersRow();

            foreach (var kv in list)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(kv.Key);
                ImGui.TableNextColumn();
                ImGui.Text($"{kv.Value.last:F2}");
                ImGui.TableNextColumn();
                // Красный, если avg больше 2 мс — заметная система
                var col = kv.Value.avg > 2.0 ? new Vector4(1f, 0.6f, 0.4f, 1f)
                        : kv.Value.avg > 0.5 ? new Vector4(1f, 0.9f, 0.6f, 1f)
                        : new Vector4(0.8f, 0.8f, 0.8f, 1f);
                ImGui.TextColored(col, $"{kv.Value.avg:F2}");
                ImGui.TableNextColumn();
                ImGui.Text($"{kv.Value.peak:F2}");
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}