using MyEngine.UI;
using System.Numerics;

public sealed class Button : UIElement
{
    public string Text = "Button";
    public Vector4? NormalTop = null;
    public Vector4? NormalBottom = null;
    public Vector4? HoverTop = null;
    public Vector4? HoverBottom = null;
    public Vector4? PressedTop = null;
    public Vector4? PressedBottom = null;
    public Vector4? TextColor = null;
    public float? TextScale = null;
    public float? CornerRadius = null;

    public override void DrawBackground(UIRenderContext ctx)
    {
        if (!Visible) return;

        var t = Theme.Current;
        var nTop = NormalTop ?? t.ButtonNormalTop;
        var nBottom = NormalBottom ?? t.ButtonNormalBottom;
        var hTop = HoverTop ?? t.ButtonHoverTop;
        var hBottom = HoverBottom ?? t.ButtonHoverBottom;
        var pTop = PressedTop ?? t.ButtonPressedTop;
        var pBottom = PressedBottom ?? t.ButtonPressedBottom;
        var corner = CornerRadius ?? t.ButtonCornerRadius;

        Vector4 top, bottom;
        if (WasClicked) { top = pTop; bottom = pBottom; }
        else if (IsHovered) { top = hTop; bottom = hBottom; }
        else { top = nTop; bottom = nBottom; }

        var offset = WasClicked ? new Vector2(0, 1) : Vector2.Zero;
        var pos = Bounds.Position + offset;

        // Тень
        if (!WasClicked)
        {
            var shadow = new Vector4(0, 0, 0, 0.35f);
            ctx.Rounded.DrawRoundedRect(
                pos + new Vector2(0, 2), Bounds.Size,
                shadow, shadow, corner);
        }

        // Кнопка
        ctx.Rounded.DrawRoundedRect(pos, Bounds.Size, top, bottom, corner);

        // Блик
        var highlight = new Vector4(1, 1, 1, 0.15f);
        ctx.Rounded.DrawRoundedRect(
            pos + new Vector2(2, 1),
            new Vector2(Bounds.Width - 4, 1),
            highlight, highlight with { W = 0f },
            corner * 0.8f);

        base.DrawBackground(ctx);
    }

    public override void DrawForeground(UIRenderContext ctx)
    {
        if (!Visible) return;

        var t = Theme.Current;
        var txtColor = TextColor ?? t.ButtonText;
        float txtScale = TextScale ?? 1f;

        var textSize = ctx.Font.MeasureText(Text, txtScale);
        var textPos = new Vector2(
            Bounds.X + (Bounds.Width - textSize.X) * 0.5f,
            Bounds.Y + (Bounds.Height - textSize.Y) * 0.5f);

        ctx.Font.DrawString(ctx.Batch, Text, textPos, txtColor, txtScale);

        base.DrawForeground(ctx);
    }
}