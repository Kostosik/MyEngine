using ImGuiNET;
using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.Diagnostics;

/// <summary>
/// Инспектор сущностей. Хранит выбранную сущность, умеет делать picking
/// (клик по миру → сущность под курсором) и рисует ImGui-окно.
/// </summary>
public sealed class EntityInspector
{
    private readonly World _world;
    private readonly Camera2D _camera;
    private readonly Func<int> _getScreenWidth;
    private readonly Func<int> _getScreenHeight;

    public Entity? Selected { get; private set; }
    public bool Visible { get; set; } = false;

    public EntityInspector(World world, Camera2D camera,
        Func<int> getScreenWidth, Func<int> getScreenHeight)
    {
        _world = world;
        _camera = camera;
        _getScreenWidth = getScreenWidth;
        _getScreenHeight = getScreenHeight;
    }

    public void Select(Entity? entity) => Selected = entity;

    /// <summary>
    /// Пикинг: клик по экрану → найти сущность под курсором.
    /// Порядок: сверху вниз (последняя в списке приоритетнее).
    /// </summary>
    public void PickAt(Vector2 screenPos)
    {
        int sw = _getScreenWidth();
        int sh = _getScreenHeight();
        var world = _camera.ScreenToWorld(screenPos, sw, sh);

        Entity? best = null;
        float bestArea = float.MaxValue;

        // Идём с конца — верхние сущности приоритетнее
        var entities = _world.Entities;
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            if (!e.IsAlive) continue;

            var t = e.Get<Transform>();
            if (t == null) continue;
            var s = e.Get<Sprite>();
            var c = e.Get<Collider>();

            Vector2 size;
            if (c != null) size = c.Size;
            else if (s != null) size = s.Size;
            else size = new Vector2(16, 16);

            var half = size * 0.5f;
            var min = t.Position - half;
            var max = t.Position + half;

            if (world.X >= min.X && world.X <= max.X &&
                world.Y >= min.Y && world.Y <= max.Y)
            {
                // Выбираем наименьшую по площади — самую "точную" цель
                float area = size.X * size.Y;
                if (area < bestArea)
                {
                    bestArea = area;
                    best = e;
                }
            }
        }

        Selected = best;
    }

    public void Draw()
    {
        if (!Visible) return;

        ImGui.SetNextWindowSize(new Vector2(380, 500), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 400, 20),
            ImGuiCond.FirstUseEver);

        ImGui.Begin("entity inspector", ImGuiWindowFlags.NoSavedSettings);

        ImGui.Text($"Selected: {(Selected != null ? $"entity #{Selected.Id}" : "none")}");

        if (ImGui.Button("Clear")) Selected = null;

        ImGui.Separator();

        if (Selected == null || !Selected.IsAlive)
        {
            ImGui.TextDisabled("Click an entity in the world to inspect.");
            ImGui.End();
            return;
        }

        // Прокручиваемая область с компонентами
        ImGui.BeginChild("components");

        foreach (var compType in Selected.ComponentTypes)
        {
            var comp = Selected.GetBoxed(compType);
            if (comp == null) continue;

            if (ImGui.CollapsingHeader(compType.Name, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ComponentDrawer.Draw(comp);
                ImGui.Unindent();
                ImGui.Separator();
            }
        }

        ImGui.EndChild();
        ImGui.End();
    }
}