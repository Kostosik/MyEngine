using System.Numerics;

namespace MyEngine.UI.Widgets;

public enum StackDirection { Vertical, Horizontal }
public enum StackAlignment { Start, Center, End }

/// <summary>
/// Контейнер, который расставляет детей в ряд (Horizontal) или колонку (Vertical).
///
/// Не рисует фон — только организует расположение. Для фона оборачивай
/// в Panel. Anchor и Offset детей игнорируются — StackPanel сам назначает Bounds.
///
/// Spacing — отступ между детьми.
/// CrossAlign — выравнивание по поперечной оси (Start / Center / End).
/// Padding* — внутренние отступы от границ контейнера.
/// </summary>
public sealed class StackPanel : UIElement
{
    public StackDirection Direction = StackDirection.Vertical;
    public float Spacing = 0;
    public StackAlignment CrossAlign = StackAlignment.Start;

    public float PaddingLeft = 0;
    public float PaddingTop = 0;
    public float PaddingRight = 0;
    public float PaddingBottom = 0;

    public override void Layout(UIRect parentBounds)
    {
        if (!Visible) return;

        // Сначала определяем свой Bounds как обычный UIElement
        Bounds = ComputeBounds(parentBounds);

        // Затем расставляем детей по своей логике
        LayoutChildren();
    }

    private void LayoutChildren()
    {
        float availW = Bounds.Width - PaddingLeft - PaddingRight;
        float availH = Bounds.Height - PaddingTop - PaddingBottom;

        if (availW < 0 || availH < 0) return;

        float cursor = 0f;

        foreach (var child in Children)
        {
            if (!child.Visible) continue;

            var childSize = child.ResolveSize();   // ← вот тут

            float cw, ch;
            if (Direction == StackDirection.Vertical)
            {
                cw = availW;
                ch = childSize.Y;
            }
            else
            {
                cw = childSize.X;
                ch = availH;
            }

            float cx, cy;
            if (Direction == StackDirection.Vertical)
            {
                cx = Bounds.X + PaddingLeft + AlignX(cw, availW);
                cy = Bounds.Y + PaddingTop + cursor;
                cursor += ch + Spacing;
            }
            else
            {
                cx = Bounds.X + PaddingLeft + cursor;
                cy = Bounds.Y + PaddingTop + AlignY(ch, availH);
                cursor += cw + Spacing;
            }

            child.Layout(new UIRect(cx, cy, cw, ch));
        }
    }

    private float AlignX(float childW, float availW) => CrossAlign switch
    {
        StackAlignment.Center => (availW - childW) * 0.5f,
        StackAlignment.End => availW - childW,
        _ => 0f,
    };

    private float AlignY(float childH, float availH) => CrossAlign switch
    {
        StackAlignment.Center => (availH - childH) * 0.5f,
        StackAlignment.End => availH - childH,
        _ => 0f,
    };
}