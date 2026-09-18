using MyEngine.InputEngine;

namespace MyEngine.Diagnostics;

/// <summary>
/// Один отладочный инструмент. Каждый независим: может иметь
/// свои hotkeys, свою отрисовку, свой жизненный цикл.
///
/// DebugOverlay — просто реестр и диспетчер.
/// </summary>
public interface IDebugTool
{
    /// <summary>Имя для отладки.</summary>
    string Name { get; }

    /// <summary>Видим ли сейчас.</summary>
    bool Visible { get; set; }

    /// <summary>Обработка клавиш. Опционально — можно оставить пустым.</summary>
    void ProcessHotkeys(Input input) { }

    /// <summary>Отрисовка ImGui. Опционально.</summary>
    void Draw() { }
}