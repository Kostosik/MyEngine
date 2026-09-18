using MyEngine.Diagnostics.Console;
using System.Text;

namespace MyEngine.Diagnostics.Console;

public static class BuiltInCommands
{
    /// <summary>Зарегистрировать встроенные команды (help, clear).</summary>
    public static void Register(ConsoleSystem console)
    {
        console.Register(new LambdaCommand("help", "Show available commands", "",
            args =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("Available commands:");
                foreach (var c in console.Commands)
                {
                    string line = $"  {c.Name} {c.ArgsHint}".TrimEnd();
                    sb.AppendLine($"{line,-40} {c.Description}");
                }
                return sb.ToString();
            }));

        console.Register(new LambdaCommand("clear", "Clear console output", "",
            args => { console.Clear(); return ""; }));
    }
}