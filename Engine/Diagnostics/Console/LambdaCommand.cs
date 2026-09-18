namespace MyEngine.Diagnostics.Console;

/// <summary>
/// Команда через лямбду. Не надо создавать класс для каждой команды.
///
/// Пример:
///   console.Register(new LambdaCommand("god", "Toggle god mode", "",
///       args => { player.Invincible = !player.Invincible; return "god toggled"; }));
/// </summary>
public sealed class LambdaCommand : IConsoleCommand
{
    public string Name { get; }
    public string Description { get; }
    public string ArgsHint { get; }
    public Func<string[], string> Handler { get; }

    public LambdaCommand(string name, string description, string argsHint,
        Func<string[], string> handler)
    {
        Name = name;
        Description = description;
        ArgsHint = argsHint;
        Handler = handler;
    }

    public string Execute(string[] args) => Handler(args);
}