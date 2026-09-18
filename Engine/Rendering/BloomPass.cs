using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Multipass bloom:
///   1. Bright pass — извлечь яркое, downsample в 1/4.
///   2. Blur X — размытие по горизонтали.
///   3. Blur Y — размытие по вертикали.
///   4. Combine — оригинал + размытое яркое.
///
/// Downsample в 1/4 даёт размытие большего радиуса за ту же стоимость
/// и естественное «широкое» свечение.
/// </summary>
public sealed class BloomPass : IPostProcessPass, IDisposable
{
    private readonly GL _gl;
    private readonly PostProcessShader _bright;
    private readonly PostProcessShader _blurX;
    private readonly PostProcessShader _blurY;
    private readonly PostProcessShader _combine;

    public BloomPass(
        GL gl,
        float threshold = 0.75f,
        float blurRadius = 1.0f,
        float strength = 1.5f,
        int downsample = 4)
    {
        _gl = gl;

        const string brightFS = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uThreshold;
out vec4 FragColor;
void main() {
    vec4 c = texture(uTexture, vUV);
    float lum = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    float mask = smoothstep(uThreshold, uThreshold + 0.1, lum);
    FragColor = vec4(c.rgb * mask, 1.0);
}";

        const string blurXFS = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uRadius;
out vec4 FragColor;
void main() {
    vec2 texel = vec2(uRadius, 0.0);
    vec3 result = vec3(0.0);
    float w[5] = float[](0.227027, 0.194594, 0.121621, 0.054054, 0.016216);
    result += texture(uTexture, vUV).rgb * w[0];
    for (int i = 1; i < 5; i++) {
        result += texture(uTexture, vUV + texel * float(i)).rgb * w[i];
        result += texture(uTexture, vUV - texel * float(i)).rgb * w[i];
    }
    FragColor = vec4(result, 1.0);
}";

        const string blurYFS = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uRadius;
out vec4 FragColor;
void main() {
    vec2 texel = vec2(0.0, uRadius);
    vec3 result = vec3(0.0);
    float w[5] = float[](0.227027, 0.194594, 0.121621, 0.054054, 0.016216);
    result += texture(uTexture, vUV).rgb * w[0];
    for (int i = 1; i < 5; i++) {
        result += texture(uTexture, vUV + texel * float(i)).rgb * w[i];
        result += texture(uTexture, vUV - texel * float(i)).rgb * w[i];
    }
    FragColor = vec4(result, 1.0);
}";

        const string combineFS = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;      // сцена
uniform sampler2D uBloom;        // размытое яркое
uniform float uStrength;
out vec4 FragColor;
void main() {
    vec4 scene = texture(uTexture, vUV);
    vec3 bloom = texture(uBloom, vUV).rgb;
    FragColor = vec4(scene.rgb + bloom * uStrength, scene.a);
}";

        _bright = new PostProcessShader(gl, brightFS, sh =>
            sh.SetFloat("uThreshold", threshold));
        _blurX = new PostProcessShader(gl, blurXFS, sh =>
            sh.SetFloat("uRadius", blurRadius));
        _blurY = new PostProcessShader(gl, blurYFS, sh =>
            sh.SetFloat("uRadius", blurRadius));
        _combine = new PostProcessShader(gl, combineFS, sh =>
        {
            sh.SetFloat("uStrength", strength);
            sh.SetInt("uBloom", 1);
        });

        _downsample = downsample;
    }

    private readonly int _downsample;

    public Texture2D Process(Texture2D input, PostProcessContext context)
    {
        int w = context.Width / _downsample;
        int h = context.Height / _downsample;

        // 1. Bright pass — downsample
        var bright = context.Rent(w, h);
        bright.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _bright.Apply(input, w, h);
        bright.Unbind(w, h);

        // 2. Blur X
        var blurX = context.Rent(w, h);
        blurX.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _blurX.Apply(bright.GetColorTextureAsTexture2D(), w, h);
        blurX.Unbind(w, h);

        // 3. Blur Y
        var blurY = context.Rent(w, h);
        blurY.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        _blurY.Apply(blurX.GetColorTextureAsTexture2D(), w, h);
        blurY.Unbind(w, h);

        // 4. Combine с оригиналом
        var final = context.Rent(context.Width, context.Height);
        final.Bind();
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // Для combine нужен особый проход — два сэмплера
        _combine.Shader.Use();
        _combine.Shader.SetInt("uTexture", 0);
        _combine.Shader.SetInt("uBloom", 1);
        _combine.Shader.SetFloat("uStrength", 1.5f);

        input.Bind(0);
        blurY.GetColorTextureAsTexture2D().Bind(1);

        // Рисуем full-screen quad через тот же VAO _combine
        _combine.DrawFullscreenQuad(context.Width, context.Height);

        final.Unbind(context.Width, context.Height);

        // Возвращаем промежуточные в пул
        context.Release(bright);
        context.Release(blurX);
        context.Release(blurY);

        return final.GetColorTextureAsTexture2D();
    }

    public void Dispose()
    {
        _bright.Dispose();
        _blurX.Dispose();
        _blurY.Dispose();
        _combine.Dispose();
    }
}