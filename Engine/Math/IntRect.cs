namespace MyEngine.Math;

/// <summary>
/// Прямоугольник в целочисленных координатах — используется для
/// описания кадров внутри спрайт-листа и регионов глифов в атласе.
/// </summary>
public readonly struct IntRect
{
    public readonly int X;
    public readonly int Y;
    public readonly int Width;
    public readonly int Height;

    public int Right => X + Width;
    public int Bottom => Y + Height;

    public IntRect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToString() => $"({X},{Y},{Width}x{Height})";
}