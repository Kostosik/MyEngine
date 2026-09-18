using System.Numerics;

namespace MyEngine.UI.Widgets;

public sealed class Label : UIElement
{
    public string Text = "";
    public Vector4 Color = Vector4.One;
    public float Scale = 1f;
    public TextAlign Align = TextAlign.Left;

    /// <summary>
    /// Размер текста в пикселях. Кэшируется — пересчитывается только
    /// при смене Text или Scale. Контекст шрифта — из Draw.
    /// </summary>
    private Vector2 _cachedSize;
    private string _cachedText = "";
    private float _cachedScale = -1f;

    public override Vector2 PreferredSize
    {
        get
        {
            // Если текст не менялся — из кэша
            if (_cachedText == Text && MathF.Abs(_cachedScale - Scale) < 0.001f)
                return _cachedSize;

            _cachedText = Text;
            _cachedScale = Scale;
            _cachedSize = Vector2.Zero;  // пока нет Font — не знаем
            return _cachedSize;
        }
    }

    /// <summary>
    /// Вызывается из Layout до расчёта размеров — но Font у нас только
    /// в UIRenderContext. Значит, auto-size Label требует «предварительного
    /// знания» шрифта. Передаём его через глобальный FontProvider.
    /// </summary>
    public override void Layout(UIRect parentBounds)
    {
        // Обновляем кэш через FontProvider, если он задан
        if (FontProvider.Current != null && AutoSizeX || AutoSizeY)
        {
            if (_cachedText != Text || MathF.Abs(_cachedScale - Scale) > 0.001f)
            {
                _cachedText = Text;
                _cachedScale = Scale;
                _cachedSize = FontProvider.Current.MeasureText(Text, Scale);
            }
        }

        base.Layout(parentBounds);
    }

    public override void Draw(UIRenderContext ctx)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;

        var size = ctx.Font.MeasureText(Text, Scale);

        float x = Bounds.X;
        if (Align == TextAlign.Center) x = Bounds.Center.X - size.X * 0.5f;
        else if (Align == TextAlign.Right) x = Bounds.Right - size.X;

        float y = Bounds.Y;

        ctx.Font.DrawString(ctx.Batch, Text,
            new Vector2(x, y), Color, Scale);
    }
}

public enum TextAlign { Left, Center, Right }