using System.Numerics;

namespace MyEngine.UI.Widgets;

/// <summary>Кнопка с текстом. Меняет цвет при наведении.</summary>
public sealed class Button : UIElement
{
    public string Text = "Button";

    public Vector4 NormalColor = new(0.2f, 0.25f, 0.35f, 1f);
    public Vector4 HoverColor = new(0.3f, 0.4f, 0.55f, 1f);
    public Vector4 PressedColor = new(0.15f, 0.2f, 0.3f, 1f);
    public Vector4 TextColor = new(1f, 1f, 1f, 1f);
    public float TextScale = 1f;

    public override void Draw(UIRenderContext ctx)
    {
        if (!Visible) return;

        // Фон по состоянию
        var color = IsHovered ? HoverColor : NormalColor;
        if (WasClicked) color = PressedColor;

        ctx.Batch.DrawRect(Bounds.Center, Bounds.Size, color);

        // Текст по центру
        var textSize = ctx.Font.MeasureText(Text, TextScale);
        var textPos = new Vector2(
            Bounds.Center.X - textSize.X * 0.5f,
            Bounds.Center.Y - textSize.Y * 0.5f);

        ctx.Font.DrawString(ctx.Batch, Text, textPos, TextColor, TextScale);

        base.Draw(ctx);
    }
}