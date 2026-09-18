using MyEngine.Rendering;

namespace MyEngine.UI;

/// <summary>
/// Глобальный доступ к текущему шрифту. Нужен, чтобы UIElement
/// мог считать размер текста во время Layout (до Draw).
///
/// Устанавливается один раз в UIRoot, когда туда присваивают Font.
/// </summary>
public static class FontProvider
{
    public static Font? Current { get; private set; }

    public static void Set(Font? font) => Current = font;
}