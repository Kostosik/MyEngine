using ImGuiNET;
using MyEngine.Ecs;
using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.Game.Systems;

public sealed class DamagePopupSystem
{
    public SystemPhase Phase => SystemPhase.PostUpdate;
    public int Priority => 0;

    private struct Popup
    {
        public Vector2 WorldPos;
        public string Text;
        public float Life;
        public float MaxLife;
        public Vector4 Color;
    }

    private readonly List<Popup> _popups = new();
    private readonly Random _rng = new();

    public void Spawn(Vector2 worldPos, int amount, Vector4? color = null)
    {
        SpawnText(worldPos, amount.ToString(), color);
    }

    public void SpawnText(Vector2 worldPos, string text, Vector4? color = null)
    {
        float jitterX = (float)(_rng.NextDouble() * 16 - 8);
        _popups.Add(new Popup
        {
            WorldPos = worldPos + new Vector2(jitterX, -20),
            Text = text,
            Life = 0.9f,
            MaxLife = 0.9f,
            Color = color ?? new Vector4(1f, 0.95f, 0.4f, 1f)
        });
    }

    public void Update(float dt)
    {
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            var p = _popups[i];
            p.Life -= dt;
            p.WorldPos.Y -= 50f * dt;
            if (p.Life <= 0f) _popups.RemoveAt(i);
            else _popups[i] = p;
        }
    }

    public void Draw(Camera2D camera)
    {
        if (_popups.Count == 0) return;

        var io = ImGui.GetIO();
        float w = io.DisplaySize.X;
        float h = io.DisplaySize.Y;

        var dl = ImGui.GetForegroundDrawList();
        foreach (var p in _popups)
        {
            float t = p.Life / p.MaxLife;
            var screen = camera.WorldToScreen(p.WorldPos, w, h);
            var col = ImGui.ColorConvertFloat4ToU32(
                new Vector4(p.Color.X, p.Color.Y, p.Color.Z, t));

            var size = ImGui.CalcTextSize(p.Text);
            dl.AddText(
                new Vector2(screen.X - size.X * 0.5f, screen.Y - size.Y * 0.5f),
                col, p.Text);
        }
    }
}