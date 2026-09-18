using System.Numerics;

namespace MyEngine.UI.Widgets;

/// <summary>Прогресс-бар: HP, XP, что угодно.</summary>
public sealed class ProgressBar : UIElement
{
    /// <summary>От 0.0 до 1.0.</summary>
    public float Value = 1f;

    public Vector4 BackgroundColor = new(0.1f, 0.1f, 0.12f, 1f);
    public Vector4 FillColor = new(0.85f, 0.25f, 0.25f, 1f);
    public Vector4 BorderColor = new(0f, 0f, 0f, 0.5f);
    public float BorderThickness = 1f;

    public override void Draw(UIRenderContext ctx)
    {
        if (!Visible) return;

        var v = System.Math.Clamp(Value, 0f, 1f);

        // Фон
        ctx.Batch.DrawRect(Bounds.Center, Bounds.Size, BackgroundColor);

        // Заливка (растёт слева)
        if (v > 0f)
        {
            float fillW = Bounds.Width * v;
            var fillCenter = new Vector2(Bounds.X + fillW * 0.5f, Bounds.Center.Y);
            ctx.Batch.DrawRect(fillCenter, new Vector2(fillW, Bounds.Height), FillColor);
        }

        // Граница
        if (BorderThickness > 0f)
        {
            float t = BorderThickness;
            ctx.Batch.DrawRect(new Vector2(Bounds.Center.X, Bounds.Top + t * 0.5f),
                new Vector2(Bounds.Width, t), BorderColor);
            ctx.Batch.DrawRect(new Vector2(Bounds.Center.X, Bounds.Bottom - t * 0.5f),
                new Vector2(Bounds.Width, t), BorderColor);
            ctx.Batch.DrawRect(new Vector2(Bounds.Left + t * 0.5f, Bounds.Center.Y),
                new Vector2(t, Bounds.Height), BorderColor);
            ctx.Batch.DrawRect(new Vector2(Bounds.Right - t * 0.5f, Bounds.Center.Y),
                new Vector2(t, Bounds.Height), BorderColor);
        }

        base.Draw(ctx);
    }
}