namespace MyEngine.Rendering.RHI.Null;

/// <summary>
/// Текстура-заглушка. Ничего не хранит на GPU, ничего не биндит.
/// Нужна для NullRenderer — чтобы игра запускалась без графики.
/// </summary>
internal sealed class NullTexture : ITexture
{
    public int Width { get; }
    public int Height { get; }

    public NullTexture(int width = 1, int height = 1)
    {
        Width = width;
        Height = height;
    }

    public void Bind(int slot = 0) { }
    public void Unbind(int slot = 0) { }
    public void Dispose() { }
}