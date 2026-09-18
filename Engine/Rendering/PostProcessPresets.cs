using Silk.NET.OpenGL;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Готовые эффекты постобработки. Каждый — статический метод,
/// который создаёт PostProcessShader с уже настроенными параметрами.
///
/// Использование:
///   var vignette = PostProcessPresets.Vignette(gl, intensity: 0.8f);
///   stack.Add(vignette);
///
/// Параметры передаются в C#-код, а не в GLSL-строку. Шейдер компилируется
/// один раз при создании.
/// </summary>
public static class PostProcessPresets
{
    // ============================================================
    // VIGNETTE — затемнение по краям
    // ============================================================

    /// <summary>
    /// Затемнение по краям экрана.
    /// intensity — насколько сильно темнеет (0..1). 0.8 — заметно, 1.0 — сильно.
    /// radius — где начинается затемнение (0..2). 1.0 — от половины, 1.4 — от краёв.
    /// color — цвет затемнения. По умолчанию чёрный.
    /// </summary>
    public static PostProcessShader Vignette(
        GL gl,
        float intensity = 0.8f,
        float radius = 1.2f,
        Vector3? color = null)
    {
        var tint = color ?? Vector3.Zero; // чёрный по умолчанию

        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uIntensity;
uniform float uRadius;
uniform vec3 uTint;
out vec4 FragColor;

void main() {
    vec4 c = texture(uTexture, vUV);
    vec2 d = vUV - vec2(0.5);
    float dist = length(d) * uRadius;
    float vig = smoothstep(1.0, 1.0 - uIntensity, dist);
    vec3 result = mix(uTint, c.rgb, vig);
    FragColor = vec4(result, c.a);
}";

        return new PostProcessShader(gl, fs, shader =>
        {
            shader.SetFloat("uIntensity", intensity);
            shader.SetFloat("uRadius", radius);
            shader.SetVector3("uTint", tint);
        });
    }

    // ============================================================
    // BLOOM — свечение ярких объектов
    // ============================================================

    /// <summary>
    /// Простой bloom: яркие пиксели размываются и складываются с оригиналом.
    /// threshold — минимальная яркость для свечения (0..1). 0.7 — норм.
    /// radius — радиус размытия в UV. 0.005 — мягкое свечение.
    /// strength — насколько сильно складывать. 2.0 — заметно.
    /// </summary>
    public static PostProcessShader Bloom(
        GL gl,
        float threshold = 0.7f,
        float radius = 0.005f,
        float strength = 2.0f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uThreshold;
uniform float uRadius;
uniform float uStrength;
out vec4 FragColor;

void main() {
    vec4 original = texture(uTexture, vUV);
    vec3 bright = max(original.rgb - uThreshold, 0.0);

    vec3 blur = vec3(0.0);
    for (int x = -2; x <= 2; x++) {
        for (int y = -2; y <= 2; y++) {
            vec2 offset = vec2(float(x), float(y)) * uRadius;
            vec4 s = texture(uTexture, vUV + offset);
            blur += max(s.rgb - uThreshold, 0.0);
        }
    }
    blur /= 25.0;

    vec3 result = original.rgb + blur * uStrength;
    FragColor = vec4(result, original.a);
}";

        return new PostProcessShader(gl, fs, shader =>
        {
            shader.SetFloat("uThreshold", threshold);
            shader.SetFloat("uRadius", radius);
            shader.SetFloat("uStrength", strength);
        });
    }

    // ============================================================
    // CHROMATIC ABERRATION — радужные обводки
    // ============================================================

    /// <summary>
    /// Смещение RGB-каналов в стороны от центра.
    /// strength — сила смещения (0.001..0.01). 0.003 — слабо заметно.
    /// </summary>
    public static PostProcessShader ChromaticAberration(GL gl, float strength = 0.003f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uStrength;
out vec4 FragColor;

void main() {
    vec2 dir = vUV - vec2(0.5);
    vec2 offset = dir * uStrength;

    float r = texture(uTexture, vUV + offset).r;
    float g = texture(uTexture, vUV).g;
    float b = texture(uTexture, vUV - offset).b;

    FragColor = vec4(r, g, b, 1.0);
}";

        return new PostProcessShader(gl, fs, shader =>
        {
            shader.SetFloat("uStrength", strength);
        });
    }

    // ============================================================
    // COLOR GRADING — тонирование сцены
    // ============================================================

    /// <summary>
    /// Умножить цвета на заданный тон. Для атмосферы.
    /// tint — множитель для R, G, B. (1,1,1) — без изменений.
    /// (1, 0.9, 0.7) — тёплая картинка. (0.7, 0.9, 1) — холодная.
    /// </summary>
    public static PostProcessShader ColorGrading(GL gl, Vector3 tint, float brightness = 1f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform vec3 uTint;
uniform float uBrightness;
out vec4 FragColor;

void main() {
    vec4 c = texture(uTexture, vUV);
    FragColor = vec4(c.rgb * uTint * uBrightness, c.a);
}";

        return new PostProcessShader(gl, fs, shader =>
        {
            shader.SetVector3("uTint", tint);
            shader.SetFloat("uBrightness", brightness);
        });
    }

    // ============================================================
    // SCANLINES — горизонтальные полосы
    // ============================================================

    /// <summary>
    /// Ретро-эффект: горизонтальные линии через каждые N пикселей.
    /// intensity — насколько тёмные линии (0..1).
    /// lineHeight — высота шага в UV (0.002 — тонкие).
    /// </summary>
    public static PostProcessShader Scanlines(GL gl, float intensity = 0.3f, float lineHeight = 0.003f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uIntensity;
uniform float uLineHeight;
out vec4 FragColor;

void main() {
    vec4 c = texture(uTexture, vUV);
    float scan = sin(vUV.y / uLineHeight * 3.14159) * 0.5 + 0.5;
    scan = mix(1.0, scan, uIntensity);
    FragColor = vec4(c.rgb * scan, c.a);
}";

        return new PostProcessShader(gl, fs, shader =>
        {
            shader.SetFloat("uIntensity", intensity);
            shader.SetFloat("uLineHeight", lineHeight);
        });
    }

    // ============================================================
    // INVERT — негатив (для теста и эффектов)
    // ============================================================

    public static PostProcessShader Invert(GL gl)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
out vec4 FragColor;
void main() {
    vec4 c = texture(uTexture, vUV);
    FragColor = vec4(1.0 - c.rgb, c.a);
}";

        return new PostProcessShader(gl, fs);
    }

    // ============================================================
    // GRAYSCALE — ч/б
    // ============================================================
    public static PostProcessShader Grayscale(GL gl, float intensity = 1f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uIntensity;
out vec4 FragColor;
void main() {
    vec4 c = texture(uTexture, vUV);
    float gray = dot(c.rgb, vec3(0.299, 0.587, 0.114));
    vec3 result = mix(c.rgb, vec3(gray), uIntensity);
    FragColor = vec4(result, c.a);
}";

        return new PostProcessShader(gl, fs, sh =>
            sh.SetFloat("uIntensity", intensity));
    }

    // ============================================================
    // SEPIA — тёплый коричневый тон
    // ============================================================
    public static PostProcessShader Sepia(GL gl, float intensity = 1f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uIntensity;
out vec4 FragColor;
void main() {
    vec4 c = texture(uTexture, vUV);
    vec3 s;
    s.r = dot(c.rgb, vec3(0.393, 0.769, 0.189));
    s.g = dot(c.rgb, vec3(0.349, 0.686, 0.168));
    s.b = dot(c.rgb, vec3(0.272, 0.534, 0.131));
    FragColor = vec4(mix(c.rgb, s, uIntensity), c.a);
}";

        return new PostProcessShader(gl, fs, sh =>
            sh.SetFloat("uIntensity", intensity));
    }

    // ============================================================
    // BLUR — гауссово размытие (упрощённое, 9 сэмплов)
    // ============================================================
    public static PostProcessShader Blur(GL gl, float radius = 0.003f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uRadius;
out vec4 FragColor;

void main() {
    // Веса для 9 сэмплов (нормированные)
    vec3 result = vec3(0.0);
    float weights[5] = float[](0.227027, 0.194594, 0.121621, 0.054054, 0.016216);

    result += texture(uTexture, vUV).rgb * weights[0];
    for (int i = 1; i < 5; i++) {
        vec2 off = vec2(float(i) * uRadius, 0.0);
        result += texture(uTexture, vUV + off).rgb * weights[i];
        result += texture(uTexture, vUV - off).rgb * weights[i];
        off = vec2(0.0, float(i) * uRadius);
        result += texture(uTexture, vUV + off).rgb * weights[i];
        result += texture(uTexture, vUV - off).rgb * weights[i];
    }
    FragColor = vec4(result, 1.0);
}";

        return new PostProcessShader(gl, fs, sh =>
            sh.SetFloat("uRadius", radius));
    }

    // ============================================================
    // PIXELATE — пикселизация
    // ============================================================
    public static PostProcessShader Pixelate(GL gl, float pixelSize = 4f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform vec2 uResolution;
uniform float uPixelSize;
out vec4 FragColor;

void main() {
    vec2 pixelPos = vUV * uResolution;
    vec2 snapped = floor(pixelPos / uPixelSize) * uPixelSize + uPixelSize * 0.5;
    vec2 newUV = snapped / uResolution;
    FragColor = texture(uTexture, newUV);
}";

        return new PostProcessShader(gl, fs, sh =>
        {
            // Разрешение пока хардкод — потом можно прокинуть
            sh.SetVector2("uResolution", new Vector2(1280, 720));
            sh.SetFloat("uPixelSize", pixelSize);
        });
    }

    // ============================================================
    // POSTERIZE — уменьшение цветов
    // ============================================================
    public static PostProcessShader Posterize(GL gl, int levels = 8)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform int uLevels;
out vec4 FragColor;
void main() {
    vec4 c = texture(uTexture, vUV);
    float lv = float(uLevels);
    vec3 result = floor(c.rgb * lv) / lv;
    FragColor = vec4(result, c.a);
}";

        return new PostProcessShader(gl, fs, sh =>
            sh.SetInt("uLevels", levels));
    }

    // ============================================================
    // WAVES — волновое искажение (подводный эффект)
    // ============================================================
    public static PostProcessShader Waves(GL gl, float amplitude = 0.01f, float frequency = 20f, float speed = 2f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uTime;
uniform float uAmplitude;
uniform float uFrequency;
uniform float uSpeed;
out vec4 FragColor;
void main() {
    vec2 offset;
    offset.x = sin(vUV.y * uFrequency + uTime * uSpeed) * uAmplitude;
    offset.y = cos(vUV.x * uFrequency + uTime * uSpeed * 0.7) * uAmplitude * 0.5;
    FragColor = texture(uTexture, vUV + offset);
}";

        return new PostProcessShader(gl, fs, sh =>
        {
            sh.SetFloat("uTime", (float)DateTime.Now.TimeOfDay.TotalSeconds);
            sh.SetFloat("uAmplitude", amplitude);
            sh.SetFloat("uFrequency", frequency);
            sh.SetFloat("uSpeed", speed);
        });
    }

    // ============================================================
    // HEAT — искажение как от горячего воздуха
    // ============================================================
    public static PostProcessShader Heat(GL gl, float strength = 0.005f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uTime;
uniform float uStrength;
out vec4 FragColor;
void main() {
    float t = uTime;
    vec2 offset;
    offset.x = sin(vUV.y * 40.0 + t * 3.0) * uStrength;
    offset.y = cos(vUV.x * 30.0 + t * 2.0) * uStrength * 0.6;
    FragColor = texture(uTexture, vUV + offset);
}";

        return new PostProcessShader(gl, fs, sh =>
        {
            sh.SetFloat("uTime", (float)DateTime.Now.TimeOfDay.TotalSeconds);
            sh.SetFloat("uStrength", strength);
        });
    }

    // ============================================================
    // DREAM — размытие + насыщенность (для снов/катсцен)
    // ============================================================
    public static PostProcessShader Dream(GL gl, float blurRadius = 0.005f, float saturation = 1.3f)
    {
        const string fs = @"#version 330 core
in vec2 vUV;
uniform sampler2D uTexture;
uniform float uBlur;
uniform float uSaturation;
out vec4 FragColor;

void main() {
    // Простое размытие 3x3
    vec3 sum = vec3(0.0);
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            vec2 off = vec2(float(x), float(y)) * uBlur;
            sum += texture(uTexture, vUV + off).rgb;
        }
    }
    sum /= 9.0;

    // Насыщенность
    float gray = dot(sum, vec3(0.299, 0.587, 0.114));
    vec3 result = mix(vec3(gray), sum, uSaturation);

    FragColor = vec4(result, 1.0);
}";

        return new PostProcessShader(gl, fs, sh =>
        {
            sh.SetFloat("uBlur", blurRadius);
            sh.SetFloat("uSaturation", saturation);
        });
    }
}