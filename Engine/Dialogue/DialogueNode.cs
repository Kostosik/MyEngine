namespace MyEngine.Dialogue;

/// <summary>
/// Один узел диалога: реплика NPC + варианты ответа.
/// Если Choices пусто — узел показывает реплику и ждёт E,
/// потом идёт в NextNode (или завершает диалог, если пусто).
/// </summary>
public sealed class DialogueNode
{
    /// <summary>Уникальный идентификатор узла.</summary>
    public string Id = "";

    /// <summary>Кто говорит. Если пусто — берётся Speaker дерева.</summary>
    public string Speaker = "";

    /// <summary>Текст реплики. Поддерживает \n.</summary>
    public string Text = "";

    /// <summary>
    /// Куда идти после прочтения, если Choices пусто.
    /// Пусто = конец диалога.
    /// </summary>
    public string NextNode = "";

    /// <summary>Варианты ответа. Если пусто — просто реплика с "E → далее".</summary>
    public List<DialogueChoice> Choices = new();

    public bool HasChoices => Choices.Count > 0;
}