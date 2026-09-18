using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// Прямоугольник UI-элемента в screen-space.
/// Позиция — левый верхний угол в пикселях.
/// </summary>
public struct UIRect
{
    public float X, Y, Width, Height;

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);
    public Vector2 Position => new(X, Y);
    public Vector2 Size => new(Width, Height);

    public UIRect(float x, float y, float w, float h)
    {
        X = x; Y = y; Width = w; Height = h;
    }

    public bool Contains(Vector2 point)
        => point.X >= X && point.X <= X + Width
        && point.Y >= Y && point.Y <= Y + Height;

    public UIRect Offset(float dx, float dy)
        => new(X + dx, Y + dy, Width, Height);

    public UIRect Inset(float left, float top, float right, float bottom)
        => new(X + left, Y + top, Width - left - right, Height - top - bottom);

    public UIRect Inset(float all)
        => Inset(all, all, all, all);
}