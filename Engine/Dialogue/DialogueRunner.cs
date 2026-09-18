using MyEngine.Events;

namespace MyEngine.Dialogue;

/// <summary>
/// Проходит по DialogueTree. Хранит текущий узел, обрабатывает выбор,
/// публикует события Started/NodeChanged/Ended.
///
/// Игра:
///   - запускает через Start(tree, context);
///   - читает CurrentNode для отображения;
///   - вызывает Advance() для "E → далее" (без выбора);
///   - вызывает Choose(i) для выбора варианта ответа.
/// </summary>
public sealed class DialogueRunner
{
    private readonly EventBus? _events;
    private DialogueTree? _tree;
    private DialogueContext? _context;
    private List<DialogueChoice> _availableChoices = new();

    public bool IsRunning { get; private set; }
    public DialogueNode? CurrentNode { get; private set; }
    public DialogueTree? CurrentTree => _tree;

    /// <summary>Доступные варианты ответа в текущем узле (с учётом условий).</summary>
    public IReadOnlyList<DialogueChoice> AvailableChoices => _availableChoices;

    public DialogueRunner(EventBus? events = null)
    {
        _events = events;
    }

    /// <summary>Начать диалог с указанного дерева.</summary>
    public void Start(DialogueTree tree, DialogueContext context)
    {
        _tree = tree;
        _context = context;

        if (!tree.HasNode(tree.StartNode))
            throw new InvalidOperationException($"Start node '{tree.StartNode}' not found in tree '{tree.Id}'");

        IsRunning = true;

        _events?.Publish(new DialogueEvent
        {
            Type = DialogueEventType.Started,
            Tree = tree
        });

        GoToNode(tree.StartNode);
    }

    /// <summary>Завершить диалог принудительно.</summary>
    public void End()
    {
        if (!IsRunning) return;

        IsRunning = false;
        var tree = _tree;
        CurrentNode = null;
        _tree = null;
        _context = null;
        _availableChoices.Clear();

        if (tree != null)
        {
            _events?.Publish(new DialogueEvent
            {
                Type = DialogueEventType.Ended,
                Tree = tree
            });
        }
    }

    /// <summary>
    /// Продвинуться вперёд в узле без выбора (когда Choices пусто).
    /// Если у узла есть выбор — метод ничего не делает (нужно Choose).
    /// </summary>
    public void Advance()
    {
        if (!IsRunning || CurrentNode == null) return;

        if (CurrentNode.HasChoices) return;

        if (string.IsNullOrEmpty(CurrentNode.NextNode))
        {
            End();
            return;
        }

        GoToNode(CurrentNode.NextNode);
    }

    /// <summary>Выбрать вариант ответа по индексу в AvailableChoices.</summary>
    public void Choose(int index)
    {
        if (!IsRunning || CurrentNode == null) return;
        if (index < 0 || index >= _availableChoices.Count) return;

        var choice = _availableChoices[index];

        // Применяем эффекты выбора
        if (_context != null)
        {
            _context.SetFlagsFromList(choice.SetFlag);
            _context.ClearFlagsFromList(choice.ClearFlag);
        }

        // Переход
        if (string.IsNullOrEmpty(choice.NextNode))
        {
            End();
            return;
        }

        GoToNode(choice.NextNode);
    }

    private void GoToNode(string id)
    {
        if (_tree == null) return;

        var node = _tree.GetNode(id);
        if (node == null)
        {
            throw new InvalidOperationException($"Node '{id}' not found in tree '{_tree.Id}'");
        }

        CurrentNode = node;
        RebuildAvailableChoices();

        _events?.Publish(new DialogueEvent
        {
            Type = DialogueEventType.NodeChanged,
            Tree = _tree,
            Node = node
        });
    }

    private void RebuildAvailableChoices()
    {
        _availableChoices.Clear();
        if (CurrentNode == null || _context == null) return;

        foreach (var choice in CurrentNode.Choices)
        {
            // Показываем только если условие выполнено (или его нет)
            if (!string.IsNullOrEmpty(choice.ShowIf) && !_context.GetFlag(choice.ShowIf))
                continue;

            _availableChoices.Add(choice);
        }
    }
}