namespace MyEngine.Dialogue;

/// <summary>
/// Диалоговое дерево: набор узлов + стартовый узел.
/// Загружается из JSON или строится в коде.
/// </summary>
public sealed class DialogueTree
{
    /// <summary>Идентификатор дерева (например, "keeper_intro").</summary>
    public string Id = "";

    /// <summary>Кто говорит по умолчанию для всех узлов.</summary>
    public string Speaker = "";

    /// <summary>Стартовый узел.</summary>
    public string StartNode = "";

    /// <summary>Все узлы дерева по их Id.</summary>
    public Dictionary<string, DialogueNode> Nodes = new();

    public DialogueNode? GetNode(string id)
        => Nodes.TryGetValue(id, out var n) ? n : null;

    public bool HasNode(string id) => Nodes.ContainsKey(id);

    /// <summary>
    /// Создать линейное дерево из массива строк.
    /// Каждая строка — узел, переход к следующей по E.
    /// Удобно для простых реплик без выбора.
    /// </summary>
    public static DialogueTree FromLines(string id, string speaker, params string[] lines)
    {
        var tree = new DialogueTree
        {
            Id = id,
            Speaker = speaker,
            StartNode = "n0"
        };

        for (int i = 0; i < lines.Length; i++)
        {
            var nodeId = $"n{i}";
            var nextId = i + 1 < lines.Length ? $"n{i + 1}" : "";
            tree.Nodes[nodeId] = new DialogueNode
            {
                Id = nodeId,
                Text = lines[i],
                NextNode = nextId
            };
        }

        return tree;
    }
}

