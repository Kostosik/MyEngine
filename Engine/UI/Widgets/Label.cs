using System.Numerics;

namespace MyEngine.UI.Widgets;

public sealed class Label : UIElement
{
    public string Text = "";
    public Vector4? Color = null;      // null → цвет из темы
    public float Scale = 1f;           // обычный float
    public TextAlign Align = TextAlign.Left;

    private Vector2 _cachedSize;
    private string? _cachedText = "";
    private float _cachedScale = float.NaN;  // NaN гарантирует первый пересчёт

    public override Vector2 PreferredSize
    {
        get
        {
            EnsureSizeCache();
            return _cachedSize;
        }
    }

    public override void Layout(UIRect parentBounds)
    {
        if ((AutoSizeX || AutoSizeY) && FontProvider.Current != null)
            EnsureSizeCache();

        base.Layout(parentBounds);
    }

    public override void DrawForeground(UIRenderContext ctx)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;

        var t = Theme.Current;
        var color = Color ?? t.TextColor;
        float scale = Scale;

        var size = ctx.Font.MeasureText(Text, scale);

        float x = Bounds.X;
        if (Align == TextAlign.Center) x = Bounds.Center.X - size.X * 0.5f;
        else if (Align == TextAlign.Right) x = Bounds.Right - size.X;

        ctx.Font.DrawString(ctx.Batch, Text,
            new Vector2(x, Bounds.Y), color, scale);

        base.DrawForeground(ctx);
    }

    private void EnsureSizeCache()
    {
        // float.NaN != float.NaN — поэтому при первом вызове условие false, пересчитаем
        if (_cachedText == Text && MathF.Abs(_cachedScale - Scale) < 0.001f)
            return;

        var font = FontProvider.Current;
        _cachedSize = font != null ? font.MeasureText(Text, Scale) : Vector2.Zero;
        _cachedText = Text;
        _cachedScale = Scale;
    }
}

public enum TextAlign { Left, Center, Right }