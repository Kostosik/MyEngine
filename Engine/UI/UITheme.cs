using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// Тема UI: цвета, размеры, шрифт по умолчанию. Одна на всю игру.
///
/// Виджеты берут значения из Theme.Current, если их собственное поле
/// не задано (null). Хочешь переопределить для конкретного элемента —
/// установи поле после создания.
///
/// Смена темы в рантайме: Theme.Current = LightTheme.Instance.
/// Все виджеты с null-полями мгновенно получат новые цвета.
/// </summary>
public sealed class UITheme
{
    // ============================================================
    // Текст
    // ============================================================

    public Vector4 TextColor = new(0.92f, 0.92f, 0.95f, 1f);
    public Vector4 TextDisabled = new(0.5f, 0.5f, 0.55f, 1f);
    public Vector4 TextAccent = new(1f, 0.9f, 0.5f, 1f);

    // ============================================================
    // Panel
    // ============================================================

    public Vector4 PanelTop = new(0.18f, 0.20f, 0.28f, 0.95f);
    public Vector4 PanelBottom = new(0.10f, 0.12f, 0.18f, 0.95f);
    public Vector4 PanelBorder = new(0.4f, 0.5f, 0.7f, 0.8f);
    public float PanelBorderThickness = 0f;
    public float PanelCornerRadius = 10f;
    public Vector4 PanelShadow = new(0f, 0f, 0f, 0.35f);
    public float PanelShadowOffset = 3f;

    // ============================================================
    // Button
    // ============================================================

    public Vector4 ButtonNormalTop = new(0.25f, 0.32f, 0.45f, 1f);
    public Vector4 ButtonNormalBottom = new(0.15f, 0.20f, 0.30f, 1f);
    public Vector4 ButtonHoverTop = new(0.35f, 0.45f, 0.65f, 1f);
    public Vector4 ButtonHoverBottom = new(0.22f, 0.30f, 0.45f, 1f);
    public Vector4 ButtonPressedTop = new(0.12f, 0.15f, 0.22f, 1f);
    public Vector4 ButtonPressedBottom = new(0.20f, 0.25f, 0.35f, 1f);
    public Vector4 ButtonText = new(1f, 1f, 1f, 1f);
    public float ButtonCornerRadius = 8f;

    // ============================================================
    // ProgressBar
    // ============================================================

    public Vector4 ProgressBackground = new(0.08f, 0.10f, 0.14f, 0.9f);
    public Vector4 ProgressFillTop = new(0.9f, 0.3f, 0.3f, 1f);
    public Vector4 ProgressFillBottom = new(0.7f, 0.15f, 0.15f, 1f);
    public Vector4 ProgressBorder = new(0f, 0f, 0f, 0.5f);
    public float ProgressCornerRadius = 6f;
    public float ProgressPadding = 2f;

    // ============================================================
    // Общие размеры
    // ============================================================

    public float DefaultSpacing = 8f;
    public float DefaultPadding = 8f;
}

/// <summary>
/// Глобальная ссылка на текущую тему.
/// </summary>
public static class Theme
{
    private static UITheme _current = DarkTheme.Instance;
    public static UITheme Current
    {
        get => _current;
        set => _current = value ?? throw new ArgumentNullException(nameof(value));
    }
}

/// <summary>Тёмная тема по умолчанию.</summary>
public static class DarkTheme
{
    public static readonly UITheme Instance = new()
    {
        TextColor = new Vector4(0.92f, 0.92f, 0.95f, 1f),
        PanelTop = new Vector4(0.18f, 0.20f, 0.28f, 0.95f),
        PanelBottom = new Vector4(0.10f, 0.12f, 0.18f, 0.95f),
        ButtonNormalTop = new Vector4(0.25f, 0.32f, 0.45f, 1f),
        ButtonNormalBottom = new Vector4(0.15f, 0.20f, 0.30f, 1f),
        ButtonHoverTop = new Vector4(0.35f, 0.45f, 0.65f, 1f),
        ButtonHoverBottom = new Vector4(0.22f, 0.30f, 0.45f, 1f),
    };
}

/// <summary>Светлая тема.</summary>
public static class LightTheme
{
    public static readonly UITheme Instance = new()
    {
        TextColor = new Vector4(0.10f, 0.10f, 0.12f, 1f),
        TextDisabled = new Vector4(0.55f, 0.55f, 0.60f, 1f),
        PanelTop = new Vector4(0.95f, 0.95f, 0.97f, 0.98f),
        PanelBottom = new Vector4(0.85f, 0.87f, 0.90f, 0.98f),
        PanelBorder = new Vector4(0.6f, 0.65f, 0.75f, 0.9f),
        PanelShadow = new Vector4(0f, 0f, 0f, 0.15f),
        ButtonNormalTop = new Vector4(0.85f, 0.88f, 0.92f, 1f),
        ButtonNormalBottom = new Vector4(0.70f, 0.75f, 0.82f, 1f),
        ButtonHoverTop = new Vector4(0.92f, 0.95f, 1f, 1f),
        ButtonHoverBottom = new Vector4(0.78f, 0.85f, 0.95f, 1f),
        ButtonPressedTop = new Vector4(0.60f, 0.65f, 0.72f, 1f),
        ButtonPressedBottom = new Vector4(0.75f, 0.80f, 0.88f, 1f),
        ButtonText = new Vector4(0.10f, 0.10f, 0.15f, 1f),
        ProgressBackground = new Vector4(0.80f, 0.82f, 0.85f, 1f),
        ProgressBorder = new Vector4(0f, 0f, 0f, 0.3f),
    };
}