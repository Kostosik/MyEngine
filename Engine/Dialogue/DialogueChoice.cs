namespace MyEngine.Dialogue;

/// <summary>
/// Один вариант ответа в узле диалога.
/// Если NextNode пустой — выбор завершает диалог.
/// </summary>
public sealed class DialogueChoice
{
    /// <summary>Текст варианта, который видит игрок.</summary>
    public string Text = "";

    /// <summary>ID следующего узла. Пустой = конец диалога.</summary>
    public string NextNode = "";

    /// <summary>
    /// Условие показа (имя флага). Если задано — вариант показывается,
    /// только если DialogueContext.GetFlag(ShowIf) == true.
    /// </summary>
    public string? ShowIf;

    /// <summary>
    /// Эффект выбора: установить флаг в true.
    /// Можно несколько через запятую: "flag1,flag2".
    /// </summary>
    public string? SetFlag;

    /// <summary>Эффект выбора: снять флаг (поставить в false).</summary>
    public string? ClearFlag;
}