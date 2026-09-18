using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.UI.Widgets;

public sealed class Image : UIElement
{
    public Texture2D? Texture;
    public Vector4 Color = Vector4.One;

    public override void Draw(UIRenderContext ctx)
    {
        if (!Visible || Texture == null) return;
        ctx.Batch.Draw(Texture, Bounds.Center, Bounds.Size, 0f, null, Color);
    }
}