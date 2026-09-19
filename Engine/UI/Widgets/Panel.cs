using MyEngine.UI;
using System.Numerics;

public class Panel : UIElement
{
    public Vector4? BackgroundTop = null;
    public Vector4? BackgroundBottom = null;
    public Vector4? BorderColor = null;
    public float? BorderThickness = null;
    public float? CornerRadius = null;
    public bool? HasShadow = null;
    public float? ShadowOffset = null;
    public Vector4? ShadowColor = null;

    public override void DrawBackground(UIRenderContext ctx)
    {
        if (!Visible) return;

        var t = Theme.Current;
        var bgTop = BackgroundTop ?? t.PanelTop;
        var bgBottom = BackgroundBottom ?? t.PanelBottom;
        var borderCol = BorderColor ?? t.PanelBorder;
        var borderThick = BorderThickness ?? t.PanelBorderThickness;
        var corner = CornerRadius ?? t.PanelCornerRadius;
        var hasShadow = HasShadow ?? true;
        var shadowOff = ShadowOffset ?? t.PanelShadowOffset;
        var shadowCol = ShadowColor ?? t.PanelShadow;

        // Тень
        if (hasShadow)
        {
            var shadowPos = Bounds.Position + new Vector2(0, shadowOff);
            ctx.Rounded.DrawRoundedRect(
                shadowPos, Bounds.Size,
                shadowCol, shadowCol, corner);
        }

        // Панель
        ctx.Rounded.DrawRoundedRect(
            Bounds.Position, Bounds.Size,
            bgTop, bgBottom, corner);

        // Верхняя подсветка
        if (borderThick > 0f)
        {
            var highlight = borderCol with { W = 0.25f };
            ctx.Rounded.DrawRoundedRect(
                Bounds.Position + new Vector2(2, 1),
                new Vector2(Bounds.Width - 4, 1),
                highlight, highlight with { W = 0f },
                corner * 0.8f);
        }

        base.DrawBackground(ctx);
    }

    // DrawForeground — базовый, обходит детей
}