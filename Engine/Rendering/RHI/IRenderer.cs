using System.Numerics;

namespace MyEngine.Rendering.RHI;

/// <summary>
/// Абстракция графического API. Одна реализация на каждый бэкенд:
/// OpenGLRenderer, VulkanRenderer, D3D12Renderer, NullRenderer.
///
/// Игра и системы рендера работают только с этим интерфейсом —
/// нигде нет прямых вызовов OpenGL, кроме реализации.
/// </summary>
public interface IRenderer : IDisposable
{
    // --- Управление кадром ---
    void Clear(Vector4 color);
    void SetViewport(int x, int y, int width, int height);
    void SetRenderTarget(IRenderTarget? target, int screenWidth, int screenHeight);

    // --- Состояние ---
    void SetBlend(bool enabled);
    void SetDepthTest(bool enabled);
    void SetScissor(bool enabled, int x = 0, int y = 0, int w = 0, int h = 0);

    // --- Фабрики ресурсов ---
    ITexture CreateTexture(byte[] rgbaPixels, int width, int height);
    ITexture CreateTexture(string path);
    ITexture CreateWhiteTexture();

    IShader CreateShader(string vertexSource, string fragmentSource);
    IShader CreateShaderFromFiles(string vertexPath, string fragmentPath);

    IRenderTarget CreateRenderTarget(int width, int height);

    // --- Отрисовка ---
    /// <summary>Нарисовать full-screen quad с текущим шейдером.</summary>
    void DrawFullscreenQuad();

    /// <summary>Нарисовать массив вершин (треугольники).</summary>
    void DrawTriangles(int vertexCount);

    /// <summary>Instanced draw.</summary>
    void DrawTrianglesInstanced(int vertexCount, int instanceCount);
}