using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace MyEngine.UI;

public sealed class ImGuiLayer : IDisposable
{
    private readonly ImGuiController _controller;

    public ImGuiLayer(GL gl, IView view, IInputContext input)
    {
        _controller = new ImGuiController(gl, view, input);
    }

    public void Update(float dt) => _controller.Update(dt);
    public void Render() => _controller.Render();
    public void Dispose() => _controller.Dispose();
}