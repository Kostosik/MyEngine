using System.Numerics;

namespace MyEngine.UI.Widgets;

public sealed class ProgressBar : UIElement
{
    /// <summary>От 0.0 до 1.0.</summary>
    public float Value = 1f;

    public Vector4? BackgroundColor = null;
    public Vector4? FillTop = null;
    public Vector4? FillBottom = null;
    public float? CornerRadius = null;
    public float? Padding = null;

    public Vector4? BorderColor = null;
    public float BorderThickness = 1f;

    public override void DrawBackground(UIRenderContext ctx)
    {
        if (!Visible) return;

        var t = Theme.Current;

        var bg = BackgroundColor ?? t.ProgressBackground;
        var fTop = FillTop ?? t.ProgressFillTop;
        var fBottom = FillBottom ?? t.ProgressFillBottom;
        var corner = CornerRadius ?? t.ProgressCornerRadius;
        var pad = Padding ?? t.ProgressPadding;

        var v = System.Math.Clamp(Value, 0f, 1f);

        ctx.Rounded.DrawRoundedRect(Bounds.Position, Bounds.Size, bg, bg, corner);

        if (v > 0.01f)
        {
            var innerPos = Bounds.Position + new Vector2(pad, pad);
            var innerSize = new Vector2(
                (Bounds.Width - pad * 2) * v,
                 Bounds.Height - pad * 2);
            float innerRadius = System.Math.Max(0f, corner - pad);

            ctx.Rounded.DrawRoundedRect(innerPos, innerSize, fTop, fBottom, innerRadius);
        }

        // рамка — если API позволяет
        if (BorderColor.HasValue && BorderThickness > 0f)
        {
            // TODO: ctx.Rounded.DrawBorder(...)
        }

        base.DrawBackground(ctx);
    }
}