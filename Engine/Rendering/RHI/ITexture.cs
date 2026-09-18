using System.Numerics;

namespace MyEngine.Rendering.RHI;

/// <summary>
/// Абстракция текстуры. Конкретные реализации: GLTexture (OpenGL),
/// VulkanTexture (будущее), NullTexture (для тестов).
///
/// Не даёт прямого доступа к GPU-handle — только операции,
/// которые нужны игре и рендер-системам.
/// </summary>
public interface ITexture : IDisposable
{
    int Width { get; }
    int Height { get; }

    /// <summary>Привязать текстуру к слоту. 0..15.</summary>
    void Bind(int slot = 0);

    /// <summary>Отвязать от слота.</summary>
    void Unbind(int slot = 0);
}