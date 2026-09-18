namespace MyEngine.Rendering.RHI;

/// <summary>
/// Цель рендера: framebuffer + текстура.
/// Позволяет рисовать не на экран, а в текстуру — для постобработки.
/// </summary>
public interface IRenderTarget : IDisposable
{
    int Width { get; }
    int Height { get; }

    /// <summary>Текстура цвета — можно рисовать её как спрайт.</summary>
    ITexture ColorTexture { get; }

    /// <summary>Сделать этот RT активным.</summary>
    void Bind();

    /// <summary>Вернуться к рендеру на экран.</summary>
    void Unbind(int screenWidth, int screenHeight);

    /// <summary>Изменить размер.</summary>
    void Resize(int width, int height);
}