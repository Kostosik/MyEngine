using MyEngine.Math;
using MyEngine.Rendering;

namespace MyEngine.Animation;

/// <summary>
/// Помощник для нарезки спрайт-листов.
/// Предполагает, что кадры одинакового размера и лежат в сетке
/// (row-major: слева-направо, сверху-вниз).
/// </summary>
public static class SpriteSheet
{
    /// <summary>
    /// Нарезать текстуру на сетку кадров размером frameWidth × frameHeight.
    /// Возвращает массив регионов в row-major порядке.
    /// </summary>
    public static IntRect[] Slice(Texture2D texture, int frameWidth, int frameHeight)
    {
        int cols = texture.Width / frameWidth;
        int rows = texture.Height / frameHeight;
        var frames = new IntRect[cols * rows];

        int i = 0;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                frames[i++] = new IntRect(x * frameWidth, y * frameHeight, frameWidth, frameHeight);

        return frames;
    }

    /// <summary>
    /// Нарезать только часть строки — полезно для персонажа с несколькими
    /// анимациями в одном PNG (например, на строке 0 — idle, на строке 1 — walk).
    /// </summary>
    public static IntRect[] SliceRow(Texture2D texture, int row, int frameWidth, int frameHeight)
    {
        int cols = texture.Width / frameWidth;
        var frames = new IntRect[cols];
        for (int x = 0; x < cols; x++)
            frames[x] = new IntRect(x * frameWidth, row * frameHeight, frameWidth, frameHeight);
        return frames;
    }
}