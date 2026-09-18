namespace MyEngine.Dialogue;

public enum DialogueEventType
{
    Started,
    NodeChanged,
    Ended
}

/// <summary>
/// Событие диалога. Публикуется DialogueRunner через EventBus.
/// </summary>
public sealed class DialogueEvent
{
    public DialogueEventType Type;
    public DialogueTree Tree = null!;
    public DialogueNode? Node;
}