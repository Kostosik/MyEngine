using System.Diagnostics;

namespace MyEngine.Ai;

/// <summary>
/// Конечный автомат (Finite State Machine) для описания поведения.
///
/// Параметризован по enum-типу состояний. Например:
///   var fsm = new StateMachine&lt;EnemyState&gt;(EnemyState.Idle);
///   fsm.AddState(EnemyState.Idle).OnTick(...);
///   fsm.AddTransition(EnemyState.Idle, EnemyState.Chase, () => canSeePlayer, priority: 1);
///
/// Универсален — не знает ни про игру, ни про конкретные сущности.
/// Подходит для AI, NPC, дверей, ловушек, квестов — всего, что имеет
/// дискретные состояния с переходами.
///
/// Игра работает с FSM через интерфейс IBehavior, поэтому конкретный
/// автомат можно потом заменить на Behavior Tree без изменения остального кода.
/// </summary>
public sealed class StateMachine<TState> where TState : struct, Enum
{
    /// <summary>Все зарегистрированные состояния по их идентификатору.</summary>
    private readonly Dictionary<TState, State<TState>> _states = new();

    /// <summary>Все возможные переходы. Перебираются на каждом Tick.</summary>
    private readonly List<Transition<TState>> _transitions = new();

    /// <summary>Текущее активное состояние.</summary>
    private TState _current;

    /// <summary>Текущее состояние. Только для чтения — менять через переходы.</summary>
    public TState Current => _current;

    /// <summary>Сколько секунд прошло с момента входа в текущее состояние. Сбрасывается при переходе.</summary>
    public float TimeInState { get; private set; }

    /// <summary>Сколько секунд прошло с момента создания автомата. Не сбрасывается.</summary>
    public float TotalTime { get; private set; }

    public StateMachine(TState initial)
    {
        _current = initial;
    }

    /// <summary>
    /// Зарегистрировать состояние. Возвращает State&lt;TState&gt; для fluent-настройки:
    ///   fsm.AddState(X).OnEnter(...).OnTick(...);
    ///
    /// ВАЖНО: все состояния, в которые может произойти переход, должны быть
    /// зарегистрированы до первого Tick. Иначе ForceTransition кинет исключение.
    /// </summary>
    public State<TState> AddState(TState id)
    {
        var s = new State<TState>(id);
        _states[id] = s;
        return s;
    }

    /// <summary>
    /// Добавить переход из конкретного состояния в другое по условию.
    /// </summary>
    /// <param name="priority">
    /// Больше — важнее. Если несколько переходов истинны одновременно,
    /// сработает тот, у которого priority максимальный. Значения по умолчанию (0)
    /// разрешают конфликты "первым добавленным" — но лучше задавать явно.
    /// </param>
    public void AddTransition(TState from, TState to, Func<bool> condition, int priority = 0)
    {
        _transitions.Add(new Transition<TState>(from, to, condition, priority));
    }

    /// <summary>
    /// Переход, который проверяется из ЛЮБОГО состояния.
    /// Классика: "если HP = 0, перейти в Dead" — не хочется описывать
    /// этот переход для каждого состояния отдельно.
    ///
    /// Обычно ставится высокий приоритет (100+), чтобы смерть перебивала
    /// остальные переходы.
    /// </summary>
    public void AddAnyTransition(TState to, Func<bool> condition, int priority = 0)
    {
        // default(TState) — заглушка для поля From, оно не используется при FromAny = true
        _transitions.Add(new Transition<TState>(default, to, condition, priority) { FromAny = true });
    }

    /// <summary>
    /// Принудительный переход, минуя проверку условий.
    /// Вызывает OnExit старого состояния, сбрасывает таймер, вызывает OnEnter нового.
    /// Кидает исключение, если состояние не зарегистрировано.
    /// </summary>
    public void ForceTransition(TState to)
    {
        if (!_states.ContainsKey(to))
            throw new InvalidOperationException($"Unknown state: {to}");

        // Выход из старого состояния (если оно было зарегистрировано)
        if (_states.TryGetValue(_current, out var fromState))
            fromState.InvokeExit();

        _current = to;
        TimeInState = 0;

        // Вход в новое состояние
        _states[_current].InvokeEnter();
    }

    /// <summary>
    /// Основной цикл автомата. Вызывается каждый кадр.
    ///
    /// Порядок работы:
    ///   1. Обновить таймеры.
    ///   2. Найти переход с наибольшим приоритетом, условие которого истинно.
    ///   3. Если переход найден — выполнить его.
    ///   4. Выполнить OnTick текущего состояния.
    ///
    /// Переход выполняется ДО OnTick — новое состояние сразу получает свой первый тик.
    /// </summary>
    public void Tick(float dt)
    {
        TotalTime += dt;
        TimeInState += dt;

        // Ищем переход с максимальным приоритетом среди всех подходящих
        Transition<TState>? best = null;
        int bestPriority = int.MinValue;

        foreach (var t in _transitions)
        {
            // Переходы "из конкретного состояния" работают только в этом состоянии.
            // Переходы "из любого" (FromAny) работают всегда.
            if (!t.FromAny && !EqualityComparer<TState>.Default.Equals(t.From, _current))
                continue;

            // Уже нашли переход с более высоким приоритетом — пропускаем
            if (t.Priority <= bestPriority) continue;

            // Условие не выполнено — не наш переход
            if (!t.Condition()) continue;

            best = t;
            bestPriority = t.Priority;
        }

        // Если нашли переход и он ведёт не в текущее состояние — выполняем
        if (best != null && !EqualityComparer<TState>.Default.Equals(best.To, _current))
            ForceTransition(best.To);

        // Тик текущего состояния — выполняется всегда, даже если только что перешли
        if (_states.TryGetValue(_current, out var state))
            state.InvokeTick(dt);
    }

    /// <summary>
    /// Вызвать OnEnter для начального состояния.
    /// Нужен один раз после всех AddState/AddTransition и перед первым Tick.
    /// Без него начальное состояние будет работать "без входа" — OnTick пойдёт,
    /// но OnEnter не вызовется.
    /// </summary>
    public void Start()
    {
        if (_states.TryGetValue(_current, out var s))
            s.InvokeEnter();
    }

    /// <summary>
    /// Объект текущего состояния — если нужно достучаться до его данных извне.
    /// Может быть null, если состояние не зарегистрировано через AddState.
    /// </summary>
    public State<TState>? CurrentState
        => _states.TryGetValue(_current, out var s) ? s : null;
}