using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Финальный проход 2D-освещения: умножение сцены на lightmap.
///
/// Реализован как IPostProcessPass, но с двумя сэмплерами.
/// Принимает сцену, отдельно получает lightmap.
///
/// ВАЖНО: lightmap передаётся через поле, устанавливается извне
/// (в MyGame.Render перед вызовом PostProcessStack.Process).
/// </summary>
public sealed class LightingPass : IPostProcessPass, IDisposable
{
    private readonly GL _gl;
    private readonly PostProcessShader _combine;
    private Texture2D? _lightmap;

    public LightingPass(GL gl)
    {
        _gl = gl;

        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;    // сцена
uniform sampler2D uLightmap;   // свет
out vec4 FragColor;

void main() {
    vec4 scene = texture(uTexture, vUV);
    vec3 light = texture(uLightmap, vUV).rgb;
    FragColor = vec4(scene.rgb * light, scene.a);
}";

        _combine = new PostProcessShader(gl, fs, sh =>
        {
            sh.SetInt("uLightmap", 1);
        });
    }

    public void SetLightmap(Texture2D lightmap) => _lightmap = lightmap;

    /// <summary>
    /// Применить lighting: сцена * lightmap.
    /// </summary>
    public unsafe Texture2D Process(Texture2D scene, PostProcessContext context)
    {
        if (_lightmap == null)
            return scene;

        var rt = context.Rent(context.Width, context.Height);
        rt.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        _gl.Disable(EnableCap.ScissorTest);
        _gl.Disable(EnableCap.Blend);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.ColorMask(true, true, true, true);
        _gl.StencilMask(0xFF);
        _gl.Viewport(0, 0, (uint)context.Width, (uint)context.Height);

        _combine.Shader.Use();
        _combine.Shader.SetInt("uTexture", 0);
        _combine.Shader.SetInt("uLightmap", 1);

        scene.Bind(0);
        _lightmap.Bind(1);

        _combine.DrawFullscreenQuad(context.Width, context.Height);

        rt.Unbind(context.Width, context.Height);
        return rt.GetColorTextureAsTexture2D();
    }


    public void Dispose() => _combine.Dispose();
}
