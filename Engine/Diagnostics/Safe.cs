namespace MyEngine.Diagnostics;

/// <summary>
/// Хелперы для опциональных подсистем: не крашат при ошибке,
/// а логируют и возвращают значение по умолчанию.
/// </summary>
public static class Safe
{
    public static void Run(string source, Action action)
    {
        try { action(); }
        catch (Exception ex) { ErrorLogger.Log(source, ex); }
    }

    public static bool TryInit(string source, Action init)
    {
        try { init(); return true; }
        catch (Exception ex) { ErrorLogger.Log(source, ex); return false; }
    }

    public static T? Try<T>(string source, Func<T> func)
    {
        try { return func(); }
        catch (Exception ex) { ErrorLogger.Log(source, ex); return default; }
    }
}