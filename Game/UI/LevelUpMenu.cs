using ImGuiNET;
using MyEngine.Ecs;
using MyEngine.Components;
using System.Numerics;

namespace MyEngine.Game.UI;

public sealed class LevelUpMenu
{
    private readonly GameState _state;
    private readonly Entity _player;

    public bool IsOpen => _state.SkillPoints > 0;

    public LevelUpMenu(GameState state, Entity player)
    {
        _state = state;
        _player = player;
    }

    public void Draw()
    {
        if (!IsOpen) return;

        var viewport = ImGui.GetIO().DisplaySize;
        var size = new Vector2(460, 260);
        ImGui.SetNextWindowPos(new Vector2(
            (viewport.X - size.X) * 0.5f,
            (viewport.Y - size.Y) * 0.5f), ImGuiCond.Always);
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);

        ImGui.Begin("Level Up!",
            ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);

        ImGui.Text($"Level {_state.PlayerLevel}. Choose a perk:");
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Button("Fortitude  (+20 max HP)", new Vector2(-1, 40)))
            ApplyPerk(Perk.Fortitude);

        ImGui.Spacing();

        if (ImGui.Button("Fury       (+3 sword damage)", new Vector2(-1, 40)))
            ApplyPerk(Perk.Fury);

        ImGui.Spacing();

        if (ImGui.Button("Agility    (+10% move speed)", new Vector2(-1, 40)))
            ApplyPerk(Perk.Agility);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.TextDisabled($"Points left: {_state.SkillPoints}");

        ImGui.End();
    }

    private enum Perk { Fortitude, Fury, Agility }

    private void ApplyPerk(Perk perk)
    {
        var hp = _player.Get<Health>();
        var attack = _player.Get<Attack>();

        switch (perk)
        {
            case Perk.Fortitude:
                if (hp != null)
                {
                    hp.MaxHp += 20;
                    hp.Hp += 20;
                    if (hp.Hp > hp.MaxHp) hp.Hp = hp.MaxHp;
                }
                break;

            case Perk.Fury:
                if (attack != null)
                    attack.Damage += 3;
                break;

            case Perk.Agility:
                _state.PlayerMoveSpeedMultiplier += 0.1f;
                break;
        }

        _state.SkillPoints--;
    }
}