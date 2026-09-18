using ImGuiNET;
using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Diagnostics;

public sealed class WorldInspectorWindow
{
    private readonly World _world;
    public bool Visible { get; set; } = false;

    private string _filter = "";

    public WorldInspectorWindow(World world)
    {
        _world = world;
    }

    public void Draw()
    {
        if (!Visible) return;

        ImGui.SetNextWindowSize(new Vector2(420, 500), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 440, 360),
            ImGuiCond.FirstUseEver);

        ImGui.Begin("world inspector",
            ImGuiWindowFlags.NoSavedSettings);

        var counts = WorldInspector.CountByComponent(_world);
        int total = counts.GetValueOrDefault("__total");

        ImGui.Text($"Total entities: {total}");
        ImGui.Separator();

        ImGui.Text("By component:");
        ImGui.Indent();

        foreach (var kv in counts.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key))
        {
            if (kv.Key == "__total") continue;
            ImGui.Text($"{kv.Key,-22} {kv.Value}");
        }
        ImGui.Unindent();

        ImGui.Separator();
        ImGui.Text("Filter by component:");
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##filter", ref _filter, 64);

        if (!string.IsNullOrWhiteSpace(_filter))
        {
            ImGui.Separator();
            ImGui.Text($"Entities containing '{_filter}':");
            ImGui.BeginChild("list");

            foreach (var e in _world.Entities)
            {
                if (!e.IsAlive) continue;

                bool match = false;
                foreach (var t in e.ComponentTypes)
                {
                    if (t.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match) continue;

                // Показать ID и типы компонентов
                ImGui.TextColored(new Vector4(1f, 0.9f, 0.5f, 1f), $"#{e.Id}");
                ImGui.SameLine();

                // Показать позицию, если есть Transform
                var t2 = e.Get<MyEngine.Components.Transform>();
                if (t2 != null)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"({t2.Position.X:F0}, {t2.Position.Y:F0})");
                }

                var names = string.Join(", ", e.ComponentTypes.Select(t => t.Name));
                ImGui.TextWrapped(names);
                ImGui.Separator();
            }

            ImGui.EndChild();
        }

        ImGui.End();
    }
}