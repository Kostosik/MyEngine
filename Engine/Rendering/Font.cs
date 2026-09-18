using MyEngine.Diagnostics;
using Silk.NET.OpenGL;
using StbTrueTypeSharp;
using System.Numerics;

namespace MyEngine.Rendering;

/// <summary>
/// Растровый шрифт: загружает TTF-файл, растеризует нужные символы
/// в один текстурный атлас и умеет рисовать текст через SpriteBatch.
///
/// Поддерживает ASCII и кириллицу. Шрифт генерируется один раз при загрузке,
/// в рантайме — только рисование по UV-регионам.
/// </summary>
public sealed class Font : IDisposable
{
    private const int AtlasWidth = 1024;
    private const int AtlasHeight = 1024;

    private readonly Dictionary<int, Glyph> _glyphs = new();
    private readonly Texture2D _atlas;
    private readonly float _scale;

    /// <summary>Высота шрифта в пикселях (кегль).</summary>
    public float Size { get; }

    /// <summary>Расстояние от верха одной строки до верха следующей.</summary>
    public float LineHeight { get; }

    /// <summary>Высота над базовой линией.</summary>
    public float Ascent { get; }

    /// <summary>Глубина под базовой линией.</summary>
    public float Descent { get; }

    public Font(GL gl, string ttfPath, float sizePx)
    {
        Size = sizePx;

        byte[] ttfData = File.ReadAllBytes(ttfPath);
        var fontInfo = StbTrueType.CreateFont(ttfData, 0)
            ?? throw new InvalidDataException($"Failed to load font: {ttfPath}");

        // Масштаб из "unit"-координат шрифта в пиксели при заданном кегле
        _scale = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, sizePx);

        int ascent = 0, descent = 0, lineGap = 0;
        // Метрики вертикали
        unsafe
        {
            StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);
        }

        Ascent = ascent * _scale;
        Descent = descent * _scale;
        LineHeight = (ascent - descent + lineGap) * _scale;

        // Растеризуем символы в атлас
        byte[] atlasPixels = new byte[AtlasWidth * AtlasHeight * 4];
        int cursorX = 1;
        int cursorY = 1;
        int rowHeight = 0;

        foreach (int codepoint in EnumerateCodepoints())
        {
            if (!Rasterize(fontInfo, codepoint, atlasPixels,
                ref cursorX, ref cursorY, ref rowHeight))
                break; // атлас переполнен
        }


        _atlas = new Texture2D(gl, atlasPixels, AtlasWidth, AtlasHeight);

        SaveAtlasDebug(atlasPixels, AtlasWidth, AtlasHeight, "font_atlas_debug.png");
    }

    /// <summary>
    /// Список всех растеризуемых символов.
    /// ASCII + кириллица. Расширяй по необходимости.
    /// </summary>
    private static IEnumerable<int> EnumerateCodepoints()
    {
        for (int c = 32; c <= 126; c++) yield return c;        // ASCII printable
        for (int c = 0x0400; c <= 0x04FF; c++) yield return c; // Cyrillic
    }

    private bool Rasterize(
        StbTrueType.stbtt_fontinfo fontInfo,
        int codepoint,
        byte[] atlasPixels,
        ref int cursorX, ref int cursorY, ref int rowHeight)
    {
        // Метрики по горизонтали
        float advance;
        unsafe {
            int advanceUnits = 0;
            int leftSideBearing = 0;
            StbTrueType.stbtt_GetCodepointHMetrics(fontInfo, codepoint,
                &advanceUnits, &leftSideBearing);
            advance = advanceUnits * _scale;
        }

        // Битрмап символа
        unsafe
        {
            int w, h, xoff, yoff;
            byte* bitmap = StbTrueType.stbtt_GetCodepointBitmap(
    fontInfo, _scale, _scale, codepoint, &w, &h, &xoff, &yoff);

            if (bitmap == null || w == 0 || h == 0)
            {
                // Пробел и подобные — без битрмапа, только advance
                _glyphs[codepoint] = new Glyph
                {
                    UV0 = Vector2.Zero,
                    UV1 = Vector2.Zero,
                    Size = Vector2.Zero,
                    Offset = Vector2.Zero,
                    Advance = advance
                };
                StbTrueType.stbtt_FreeBitmap(bitmap, null);
                return true;
            }

            // Проверка, влезает ли глиф в текущую строку
            if (cursorX + w + 1 >= AtlasWidth)
            {
                cursorX = 1;
                cursorY += rowHeight + 1;
                rowHeight = 0;
            }

            // Атлас переполнен
            if (cursorY + h + 1 >= AtlasHeight)
            {
                StbTrueType.stbtt_FreeBitmap(bitmap, null);
                return false;
            }


            // === ДИАГНОСТИКА ===
            if (codepoint == 'A')
            {
                byte minA = 255, maxA = 0;
                for (int i = 0; i < w * h; i++)
                {
                    byte a = bitmap[i];
                    if (a < minA) minA = a;
                    if (a > maxA) maxA = a;
                }
                Log.Info("Font", $"'A': scale={_scale:F4}, w={w}, h={h}, xoff={xoff}, yoff={yoff}, minA={minA}, maxA={maxA}");
            }
            // Копируем bitmap в атлас (RGB белые, A = alpha из шрифта)
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte alpha = bitmap[y * w + x];
                    int dstIdx = ((cursorY + y) * AtlasWidth + (cursorX + x)) * 4;
                    atlasPixels[dstIdx + 0] = 255;
                    atlasPixels[dstIdx + 1] = 255;
                    atlasPixels[dstIdx + 2] = 255;
                    atlasPixels[dstIdx + 3] = alpha;
                }
            }

            // Записываем метрики
            _glyphs[codepoint] = new Glyph
            {
                UV0 = new Vector2(cursorX / (float)AtlasWidth, cursorY / (float)AtlasHeight),
                UV1 = new Vector2((cursorX + w) / (float)AtlasWidth, (cursorY + h) / (float)AtlasHeight),
                Size = new Vector2(w, h),
                Offset = new Vector2(xoff, yoff),
                Advance = advance
            };

            StbTrueType.stbtt_FreeBitmap(bitmap, null);

            cursorX += w + 1;
            if (h > rowHeight) rowHeight = h;
        }

        return true;
    }

    /// <summary>
    /// Размер строки в пикселях при данном масштабе.
    /// Учитывает переносы строк.
    /// </summary>
    public Vector2 MeasureText(string text, float scale = 1f)
    {
        float maxWidth = 0;
        float curWidth = 0;
        float height = LineHeight * scale;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (ch == '\n')
            {
                if (curWidth > maxWidth) maxWidth = curWidth;
                curWidth = 0;
                height += LineHeight * scale;
                continue;
            }

            if (!_glyphs.TryGetValue(ch, out var g)) continue;
            curWidth += g.Advance * scale;
        }

        if (curWidth > maxWidth) maxWidth = curWidth;
        return new Vector2(maxWidth, height);
    }

    /// <summary>
    /// Нарисовать строку через SpriteBatch.
    /// position — левый верхний угол текстовой области (не baseline).
    /// Поддерживает '\n'.
    /// </summary>
    public void DrawString(
        SpriteBatch batch,
        string text,
        Vector2 position,
        Vector4 color,
        float scale = 1f)
    {
        float penX = position.X;
        // Baseline = верх текста + Ascent
        float baselineY = position.Y + Ascent * scale;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (ch == '\n')
            {
                penX = position.X;
                baselineY += LineHeight * scale;
                continue;
            }

            if (!_glyphs.TryGetValue(ch, out var g)) continue;
            if (g.Size.X <= 0 || g.Size.Y <= 0)
            {
                // Пробел и подобные — просто двигаем курсор
                penX += g.Advance * scale;
                continue;
            }

            // Позиция верхнего левого угла глифа
            var glyphTopLeft = new Vector2(
                penX + g.Offset.X * scale,
                baselineY + g.Offset.Y * scale);

            // SpriteBatch рисует от центра — пересчитываем
            var glyphSize = g.Size * scale;
            var glyphCenter = glyphTopLeft + glyphSize * 0.5f;

            batch.Draw(
                _atlas,
                glyphCenter,
                glyphSize,
                g.UV0,
                g.UV1,
                0f,
                new Vector2(0.5f),
                color);

            penX += g.Advance * scale;
        }
    }

    private static void SaveAtlasDebug(byte[] pixels, int w, int h, string path)
    {
        // Простой PPM-экспорт (открывается XnView, GIMP, ImageMagick)
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        // PPM RGB — конвертируем RGBA → RGB, alpha как белый фон
        bw.Write(System.Text.Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n"));
        for (int i = 0; i < pixels.Length; i += 4)
        {
            // Композитим на чёрный фон по alpha, чтобы увидеть, где есть глифы
            byte a = pixels[i + 3];
            bw.Write(pixels[i + 0]);  // R
            bw.Write(pixels[i + 1]);  // G
            bw.Write(pixels[i + 2]);  // B
        }
    }

    public void Dispose() => _atlas.Dispose();
}