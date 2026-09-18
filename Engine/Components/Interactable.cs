namespace MyEngine.Components;

/// <summary>
/// Объект, с которым можно взаимодействовать (NPC, сундук, дверь, кнопка).
/// Игра описывает, что произойдёт при взаимодействии — через подписку
/// на InteractionRequestedEvent.
/// </summary>
public sealed class Interactable
{
    /// <summary>Логический идентификатор для игровой логики: "lighthouse", "chest_01".</summary>
    public string Id = "";

    /// <summary>Радиус, в котором объект ловит взаимодействие.</summary>
    public float Radius = 60f;

    /// <summary>Текст подсказки: "Talk", "Open", "Inspect".</summary>
    public string Prompt = "Interact";

    /// <summary>Кто говорит или что это (для UI).</summary>
    public string Speaker = "";
}