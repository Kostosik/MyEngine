namespace MyEngine.Diagnostics.Console;

/// <summary>
/// Команда консоли. Реализации регистрируются в ConsoleSystem.
/// Выполняется при вводе имени команды в консоль.
/// </summary>
public interface IConsoleCommand
{
    /// <summary>Имя команды, по которому её вызывают (без пробелов).</summary>
    string Name { get; }

    /// <summary>Краткое описание для подсказки.</summary>
    string Description { get; }

    /// <summary>
    /// Список аргументов. Для подсказки: [name] [count]
    /// Пустая строка — команда без аргументов.
    /// </summary>
    string ArgsHint { get; }

    /// <summary>
    /// Выполнить команду. args — уже разбитые по пробелам аргументы
    /// (без имени команды). Возвращает строку-результат для вывода в консоль.
    /// </summary>
    string Execute(string[] args);
}