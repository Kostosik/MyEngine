// MyEngine.Engine.Tests/NullRendererTests.cs

using MyEngine.Rendering.RHI;
using MyEngine.Rendering.RHI.Null;
using System.Numerics;

namespace MyEngine.Tests;

public class NullRendererTests
{
    [Fact]
    public void NullRenderer_Implements_IRenderer()
    {
        using var renderer = new NullRenderer();
        Assert.NotNull(renderer);
    }

    [Fact]
    public void NullRenderer_CreatesResources()
    {
        var r = new NullRenderer();

        var tex = r.CreateTexture(new byte[4], 1, 1);
        Assert.NotNull(tex);
        Assert.Equal(1, tex.Width);
        Assert.Equal(1, tex.Height);

        var shader = r.CreateShader("void main() {}", "void main() {}");
        Assert.NotNull(shader);

        var rt = r.CreateRenderTarget(1280, 720);
        Assert.NotNull(rt);
        Assert.Equal(1280, rt.Width);
        Assert.Equal(720, rt.Height);
    }

    [Fact]
    public void NullRenderer_DrawCalls_DoNotThrow()
    {
        var r = new NullRenderer();
        r.Clear(Vector4.Zero);
        r.SetViewport(0, 0, 1920, 1080);
        r.SetBlend(true);
        r.SetDepthTest(false);
        r.DrawFullscreenQuad();
        r.DrawTriangles(100);
    }

    [Fact]
    public void NullRenderer_ResourcesDoNotThrow()
    {
        var r = new NullRenderer();
        var tex = r.CreateTexture(new byte[4], 1, 1);
        tex.Bind(0);
        tex.Unbind(0);
        tex.Dispose();
    }
}