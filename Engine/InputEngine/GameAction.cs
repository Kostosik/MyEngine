namespace MyEngine.InputEngine;

/// <summary>
/// Игровые действия. Не путать с debug-хоткеями — те обрабатываются
/// напрямую через Key.FX в DebugOverlay и MyGame.
/// </summary>
public enum GameAction
{
    // === Движение ===
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,

    // === Бой ===
    Attack,

    // === Взаимодействие ===
    Interact,

    // === Системное ===
    Restart,
    // Escape — через Key.Escape напрямую, не в GameAction
    Jump,
}