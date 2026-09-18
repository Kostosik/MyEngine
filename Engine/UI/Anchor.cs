namespace MyEngine.UI;

/// <summary>
/// Якорь — к какой стороне родителя привязан элемент.
/// Комбинируется с пиксельным смещением (Offset).
/// </summary>
[Flags]
public enum Anchor
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Top = 1 << 2,
    Bottom = 1 << 3,
    CenterX = 1 << 4,
    CenterY = 1 << 5,

    TopLeft = Top | Left,
    TopCenter = Top | CenterX,
    TopRight = Top | Right,
    MiddleLeft = CenterY | Left,
    MiddleCenter = CenterY | CenterX,
    MiddleRight = CenterY | Right,
    BottomLeft = Bottom | Left,
    BottomCenter = Bottom | CenterX,
    BottomRight = Bottom | Right,

    StretchHorizontal = Left | Right,
    StretchVertical = Top | Bottom,
    StretchAll = Left | Right | Top | Bottom
}