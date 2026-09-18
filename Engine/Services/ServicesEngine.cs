namespace MyEngine.Services;

/// <summary>
/// Глобальный контейнер сервисов. Один на процесс.
///
/// Использование:
///   Services.Register(new AudioManager(...));
///   var audio = Services.Get&lt;AudioManager&gt;();
///
/// Сервисы — это долгоживущие объекты, которые нужны многим системам.
/// Не путать с ECS-компонентами — это другое.
///
/// Статический — потому что сервисы создаются один раз на игру,
/// а доступ к ним нужен везде. Альтернатива — DI-контейнер,
/// но для нашего размера это оверкилл.
/// </summary>
public static class ServicesEngine
{
    private static readonly Dictionary<Type, object> _services = new();
    private static readonly object _lock = new();

    /// <summary>Зарегистрировать сервис. Заменяет существующий, если был.</summary>
    public static void Register<T>(T service) where T : class
    {
        lock (_lock)
            _services[typeof(T)] = service;
    }

    /// <summary>Зарегистрировать по интерфейсу: Register&lt;IAudio&gt;(audioManager).</summary>
    public static void Register<TInterface, TImpl>(TImpl service)
        where TInterface : class
        where TImpl : class, TInterface
    {
        lock (_lock)
            _services[typeof(TInterface)] = service;
    }

    /// <summary>Получить сервис. Кидает исключение, если не зарегистрирован.</summary>
    public static T Get<T>() where T : class
    {
        lock (_lock)
        {
            if (_services.TryGetValue(typeof(T), out var s))
                return (T)s;
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }
    }

    /// <summary>Попробовать получить. Возвращает null, если нет.</summary>
    public static T? TryGet<T>() where T : class
    {
        lock (_lock)
            return _services.TryGetValue(typeof(T), out var s) ? (T)s : null;
    }

    /// <summary>Проверить наличие.</summary>
    public static bool Has<T>() where T : class
    {
        lock (_lock)
            return _services.ContainsKey(typeof(T));
    }

    /// <summary>Удалить сервис.</summary>
    public static void Remove<T>() where T : class
    {
        lock (_lock)
            _services.Remove(typeof(T));
    }

    /// <summary>Очистить всё. При завершении приложения.</summary>
    public static void Clear()
    {
        lock (_lock)
            _services.Clear();
    }
}