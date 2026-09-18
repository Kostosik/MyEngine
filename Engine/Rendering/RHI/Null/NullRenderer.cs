using System.Numerics;

namespace MyEngine.Rendering.RHI.Null;

/// <summary>
/// Заглушка IRenderer. Никаких вызовов GPU, никакой графики.
///
/// Использование:
///   - Headless тесты и CI без окна.
///   - Запуск серверной логики (мультиплеер, симуляция).
///   - Проверка, что игровой код не зависит от рендера.
///
/// ВСЕ фабрики возвращают null-ресурсы, все draw-вызовы игнорируются.
/// </summary>
public sealed class NullRenderer : IRenderer
{
    public void Clear(Vector4 color) { }
    public void SetViewport(int x, int y, int width, int height) { }
    public void SetRenderTarget(IRenderTarget? target, int screenWidth, int screenHeight) { }

    public void SetBlend(bool enabled) { }
    public void SetDepthTest(bool enabled) { }
    public void SetScissor(bool enabled, int x = 0, int y = 0, int w = 0, int h = 0) { }

    public ITexture CreateTexture(byte[] rgbaPixels, int width, int height)
        => new NullTexture(width, height);

    public ITexture CreateTexture(string path)
    {
        // Пробуем прочитать размеры файла, чтобы не ломать код,
        // который полагается на Width/Height. Если файла нет — 1×1.
        try
        {
            var bytes = File.ReadAllBytes(path);
            return new NullTexture(1, 1);
        }
        catch
        {
            return new NullTexture(1, 1);
        }
    }

    public ITexture CreateWhiteTexture() => new NullTexture(1, 1);

    public IShader CreateShader(string vertexSource, string fragmentSource)
        => new NullShader();

    public IShader CreateShaderFromFiles(string vertexPath, string fragmentPath)
        => new NullShader();

    public IRenderTarget CreateRenderTarget(int width, int height)
        => new NullRenderTarget(width, height);

    public void DrawFullscreenQuad() { }
    public void DrawTriangles(int vertexCount) { }
    public void DrawTrianglesInstanced(int vertexCount, int instanceCount) { }

    public void Dispose() { }
}