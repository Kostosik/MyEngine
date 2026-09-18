using System.Text;

namespace MyEngine.Diagnostics;

public static class ErrorLogger
{
    public static void Initialize(string logPath)
    {
        Diagnostics.Log.Initialize(logPath);
    }

    public static void Log(string source, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append(ex.Message);
        if (ex.StackTrace != null)
        {
            sb.AppendLine();
            sb.Append(ex.StackTrace);
        }

        Diagnostics.Log.Error(source, sb.ToString());
    }

    public static void Log(string source, string message)
    {
        Diagnostics.Log.Info(source, message);
    }
}