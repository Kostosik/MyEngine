using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// Что нужно UI для отрисовки: SpriteBatch, шрифт, разрешение.
/// Передаётся в Draw каждого элемента.
/// </summary>
public sealed class UIRenderContext
{
    public SpriteBatch Batch = null!;
    public Font Font = null!;
    public int ScreenWidth;
    public int ScreenHeight;
    public Texture2D White = null!;
}