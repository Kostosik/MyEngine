using ImGuiNET;
using System.Numerics;

namespace TestRPGGame.UI;

public sealed class GameOverScreen
{
    private readonly GameState _state;

    public bool RestartRequested { get; private set; }

    public GameOverScreen(GameState state)
    {
        _state = state;
    }

    public void Draw()
    {
        if (_state.Phase == GamePhase.Playing) return;

        var io = ImGui.GetIO();

        // Затемняем фон на 55%
        var bg = ImGui.GetBackgroundDrawList();
        bg.AddRectFilled(
            Vector2.Zero, io.DisplaySize,
            ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)));

        bool victory = _state.Phase == GamePhase.Victory;

        var size = new Vector2(440, 280);
        ImGui.SetNextWindowPos(new Vector2(
            (io.DisplaySize.X - size.X) * 0.5f,
            (io.DisplaySize.Y - size.Y) * 0.5f), ImGuiCond.Always);
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);

        ImGui.Begin(victory ? "Victory!" : "You Died",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus);

        if (victory)
            ImGui.TextColored(new Vector4(0.45f, 1f, 0.45f, 1f),
                "The lighthouse burns bright again.");
        else
            ImGui.TextColored(new Vector4(1f, 0.45f, 0.45f, 1f),
                "The darkness takes you...");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text($"Level reached:   {_state.PlayerLevel}");
        ImGui.Text($"Enemies killed:  {_state.EnemiesKilled} / {_state.EnemiesTotal}");

        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Button("Restart    [R]", new Vector2(-1, 48)))
            RestartRequested = true;

        ImGui.Spacing();
        ImGui.TextDisabled("Press R to start a new run");

        ImGui.End();
    }

    public void ClearRequest() => RestartRequested = false;
}