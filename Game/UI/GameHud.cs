using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Game.Components;
using MyEngine.UI;
using MyEngine.UI.Widgets;
using System.Drawing;
using System.Numerics;

namespace TestRPGGame.UI;

/// <summary>
/// Игровой HUD: HP-бар, XP-бар, уровень, искры, очки навыков.
/// Строится один раз, значения обновляются каждый кадр через Update.
/// </summary>
public sealed class GameHud : Panel
{
    private readonly GameState _state;
    private readonly Entity _player;

    private ProgressBar _hpBar = null!;
    private Label _hpText = null!;
    private ProgressBar _xpBar = null!;
    private Label _xpText = null!;
    private Label _levelText = null!;
    private Label _sparksText = null!;
    private Label _pointsText = null!;

    public GameHud(GameState state, Entity player)
    {
        _state = state;
        _player = player;

        // Корневая панель HUD
        Anchor = Anchor.TopLeft;
        Offset = new Vector2(16, 16);
        Size = new Vector2(360, 130);
        BackgroundTop = new Vector4(0.06f, 0.07f, 0.10f, 0.82f);
        BackgroundBottom = new Vector4(0.06f, 0.07f, 0.10f, 0.82f);
        BorderColor = new Vector4(0.35f, 0.40f, 0.50f, 0.9f);
        BorderThickness = 2f;

        BuildLayout();
    }

    private void BuildLayout()
    {
        // --- Заголовок: уровень ---
        _levelText = Add(new Label
        {
            Text = "Lv. 1",
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 8),
            Size = new Vector2(80, 24),
            Color = new Vector4(1f, 0.9f, 0.5f, 1f),
            Scale = 1.2f,
            Align = TextAlign.Left
        });

        // --- HP-бар ---
        _hpBar = Add(new ProgressBar
        {
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 40),
            Size = new Vector2(336, 24),
            Value = 1f,
            BackgroundColor = new Vector4(0.10f, 0.05f, 0.05f, 0.9f),
            FillTop = new Vector4(0.85f, 0.25f, 0.25f, 1f),
            FillBottom= new Vector4(0.85f, 0.25f, 0.25f, 1f),
            BorderColor = new Vector4(0f, 0f, 0f, 0.9f),
            BorderThickness = 2f
        });

        _hpText = Add(new Label
        {
            Text = "100 / 100",
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 40),
            Size = new Vector2(336, 24),
            Color = new Vector4(1f, 1f, 1f, 1f),
            Scale = 0.9f,
            Align = TextAlign.Center
        });

        // --- XP-бар ---
        _xpBar = Add(new ProgressBar
        {
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 70),
            Size = new Vector2(336, 16),
            Value = 0f,
            BackgroundColor = new Vector4(0.05f, 0.05f, 0.10f, 0.9f),
            FillTop = new Vector4(0.85f, 0.25f, 0.25f, 1f),
            FillBottom = new Vector4(0.85f, 0.25f, 0.25f, 1f),
            BorderColor = new Vector4(0f, 0f, 0f, 0.9f),
            BorderThickness = 2f
        });

        _xpText = Add(new Label
        {
            Text = "0 / 120",
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 70),
            Size = new Vector2(336, 16),
            Color = new Vector4(0.9f, 0.95f, 1f, 1f),
            Scale = 0.75f,
            Align = TextAlign.Center
        });

        // --- Искры ---
        _sparksText = Add(new Label
        {
            Text = "Sparks: 0 / 0",
            Anchor = Anchor.TopLeft,
            Offset = new Vector2(12, 96),
            Size = new Vector2(160, 20),
            Color = new Vector4(1f, 0.95f, 0.4f, 1f),
            Scale = 0.95f,
            Align = TextAlign.Left
        });

        // --- Очки навыков ---
        _pointsText = Add(new Label
        {
            Text = "",
            Anchor = Anchor.TopRight,
            Offset = new Vector2(-12, 96),
            Size = new Vector2(180, 20),
            Color = new Vector4(0.6f, 1f, 0.7f, 1f),
            Scale = 0.95f,
            Align = TextAlign.Right
        });
    }

    /// <summary>
    /// Обновить значения HUD из текущего состояния. Вызывать каждый кадр.
    /// </summary>
    public void Refresh()
    {
        var hp = _player.Get<Health>();

        // HP
        if (hp != null)
        {
            _hpBar.Value = hp.MaxHp > 0 ? (float)hp.Hp / hp.MaxHp : 0f;
            _hpText.Text = $"{hp.Hp} / {hp.MaxHp}";
        }

        // XP
        _xpBar.Value = _state.XpToNext > 0
            ? (float)_state.PlayerXp / _state.XpToNext
            : 0f;
        _xpText.Text = $"{_state.PlayerXp} / {_state.XpToNext}";
        _levelText.Text = $"Lv. {_state.PlayerLevel}";

        // Sparks
        _sparksText.Text = $"Sparks: {_state.SparksCollected} / {_state.SparksTotal}";

        // Skill points
        _pointsText.Text = _state.SkillPoints > 0
            ? $"Points: {_state.SkillPoints}"
            : "";
    }
}