using System.Numerics;

namespace MyEngine.UI.Widgets;

/// <summary>Панель — прямоугольник с фоном. Может содержать детей.</summary>
public class Panel : UIElement
{
    public Vector4 BackgroundColor = new(0.1f, 0.1f, 0.12f, 0.85f);
    public Vector4 BorderColor = new(0.3f, 0.3f, 0.35f, 1f);
    public float BorderThickness = 0f;

    public override void Draw(UIRenderContext ctx)
    {
        if (!Visible) return;

        // Фон
        var center = Bounds.Center;
        ctx.Batch.DrawRect(center, Bounds.Size, BackgroundColor);

        // Граница (4 тонких прямоугольника)
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