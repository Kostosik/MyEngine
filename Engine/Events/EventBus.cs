namespace MyEngine.Events;

/// <summary>
/// Шина событий. Универсальная — не знает про игру.
///
/// Улучшения по сравнению с базовой версией:
///   - Приоритеты: подписчики вызываются в порядке убывания приоритета.
///   - Отписка через IDisposable: using (events.Subscribe&lt;T&gt;(...)) { ... }
///   - Статистика: сколько подписок, сколько событий опубликовано.
/// </summary>
public sealed class EventBus
{
    private class Subscriber
    {
        public Delegate Handler = null!;
        public int Priority;
        public bool Once;
        public bool Removed;
    }

    private class Channel
    {
        public List<Subscriber> Subscribers = new();
        public bool Dirty; // требует пересортировки
    }

    private readonly Dictionary<Type, Channel> _channels = new();
    private long _publishedCount;
    private long _deliveredCount;

    public long PublishedCount => _publishedCount;
    public long DeliveredCount => _deliveredCount;
    public int ChannelCount => _channels.Count;

    /// <summary>
    /// Подписаться на событие.
    /// priority — больше значит раньше. По умолчанию 0.
    /// Возвращает IDisposable для отписки через using.
    /// </summary>
    public IDisposable Subscribe<T>(Action<T> handler, int priority = 0) where T : class
    {
        var ch = GetChannel<T>();
        var sub = new Subscriber { Handler = handler, Priority = priority };
        ch.Subscribers.Add(sub);
        ch.Dirty = true;
        return new Subscription(this, typeof(T), sub);
    }

    /// <summary>Подписаться на одно событие — после первого вызова отпишется.</summary>
    public IDisposable SubscribeOnce<T>(Action<T> handler, int priority = 0) where T : class
    {
        var ch = GetChannel<T>();
        var sub = new Subscriber { Handler = handler, Priority = priority, Once = true };
        ch.Subscribers.Add(sub);
        ch.Dirty = true;
        return new Subscription(this, typeof(T), sub);
    }

    /// <summary>Опубликовать событие. Вызывает всех подписчиков по приоритету.</summary>
    public void Publish<T>(T evt) where T : class
    {
        _publishedCount++;

        if (!_channels.TryGetValue(typeof(T), out var ch)) return;

        if (ch.Dirty)
        {
            ch.Subscribers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            ch.Dirty = false;
        }

        // Идём по индексу, чтобы можно было безопасно помечать Removed
        for (int i = 0; i < ch.Subscribers.Count; i++)
        {
            var sub = ch.Subscribers[i];
            if (sub.Removed) continue;

            try
            {
                ((Action<T>)sub.Handler)(evt);
                _deliveredCount++;
            }
            catch (Exception ex)
            {
                MyEngine.Diagnostics.Log.Error("EventBus",
                    $"Handler for {typeof(T).Name} threw: {ex.Message}", ex);
            }

            if (sub.Once) sub.Removed = true;
        }

        // Очищаем удалённых раз в N
        ch.Subscribers.RemoveAll(s => s.Removed);
    }

    /// <summary>Отписаться вручную, если не хочешь возиться с IDisposable.</summary>
    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        if (!_channels.TryGetValue(typeof(T), out var ch)) return;
        ch.Subscribers.RemoveAll(s => s.Handler.Equals(handler));
    }

    /// <summary>Убрать все подписки. Используется при рестарте.</summary>
    public void Clear()
    {
        _channels.Clear();
        _publishedCount = 0;
        _deliveredCount = 0;
    }

    /// <summary>Сколько подписок на событие типа T.</summary>
    public int SubscriberCount<T>() where T : class
        => _channels.TryGetValue(typeof(T), out var ch) ? ch.Subscribers.Count : 0;

    private Channel GetChannel<T>() where T : class
    {
        if (!_channels.TryGetValue(typeof(T), out var ch))
        {
            ch = new Channel();
            _channels[typeof(T)] = ch;
        }
        return ch;
    }

    private void Remove(Type type, Subscriber sub)
    {
        if (_channels.TryGetValue(type, out var ch))
            sub.Removed = true;
    }

    private sealed class Subscription : IDisposable
    {
        private readonly EventBus _bus;
        private readonly Type _type;
        private readonly Subscriber _sub;
        private bool _disposed;

        public Subscription(EventBus bus, Type type, Subscriber sub)
        {
            _bus = bus;
            _type = type;
            _sub = sub;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _bus.Remove(_type, _sub);
        }
    }
}