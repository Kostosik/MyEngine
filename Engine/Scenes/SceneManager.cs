using MyEngine.Diagnostics;

namespace MyEngine.Scenes;

/// <summary>
/// Менеджер сцен. Хранит стек сцен, обрабатывает переходы.
///
/// Операции:
///   Load(name) — заменить всю сцену (стек становится [name]).
///   Push(name) — положить сцену поверх (для меню паузы).
///   Pop()      — снять верхнюю сцену (вернуться к предыдущей).
///
/// Переходы отложены до следующего Update — можно безопасно
/// вызывать из середины Update или из кнопок ImGui.
/// </summary>
public sealed class SceneManager
{
    private readonly Dictionary<string, Func<Scene>> _registry = new();
    private readonly List<Scene> _stack = new();
    private Application _app = null!;

    // Отложенный переход. Формат: "replace:name", "push:name", "pop"
    private string? _pending;

    /// <summary>Верхняя (активная) сцена. Null, если стек пуст.</summary>
    public Scene? Current => _stack.Count > 0 ? _stack[^1] : null;

    /// <summary>Количество сцен в стеке.</summary>
    public int StackCount => _stack.Count;

    /// <summary>Все зарегистрированные имена сцен.</summary>
    public IEnumerable<string> RegisteredScenes => _registry.Keys;

    // ============================================================
    // Инициализация
    // ============================================================

    /// <summary>Привязать к Application. Вызывается движком один раз.</summary>
    public void Attach(Application app) => _app = app;

    /// <summary>Зарегистрировать сцену по имени. Фабрика создаёт новый экземпляр.</summary>
    public void Register(string name, Func<Scene> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Scene name must not be empty");

        _registry[name] = factory;
    }

    /// <summary>Зарегистрировать сцену типа T.</summary>
    public void Register<T>(string name) where T : Scene, new()
        => Register(name, () => new T());

    // ============================================================
    // Операции (откладываются до следующего Update)
    // ============================================================

    /// <summary>Заменить всю сцену: стек становится [name].</summary>
    public void Load(string name)
    {
        EnsureRegistered(name);
        _pending = "replace:" + name;
    }

    /// <summary>Положить сцену поверх текущей (для меню, паузы).</summary>
    public void Push(string name)
    {
        EnsureRegistered(name);
        _pending = "push:" + name;
    }

    /// <summary>Снять верхнюю сцену и вернуться к предыдущей.</summary>
    public void Pop()
    {
        if (_stack.Count == 0) return;
        _pending = "pop";
    }

    // ============================================================
    // Тик (вызывается движком)
    // ============================================================

    public void Update(float dt)
    {
        ApplyPendingTransition();

        if (_stack.Count == 0) return;

        // Обновляем верхнюю сцену
        Current!.Update(_app, dt);

        // Если верхняя — пауза, могут быть фоновые сцены,
        // у которых PauseBelow = false
        for (int i = _stack.Count - 2; i >= 0; i--)
        {
            if (_stack[i].PauseBelow) break;
            _stack[i].Update(_app, dt);
        }
    }

    public void UpdateVariable(float dt)
    {
        if (_stack.Count == 0) return;
        Current!.UpdateVariable(_app, dt);
    }

    public void Render()
    {
        if (_stack.Count == 0) return;

        // Рисуем нижние сцены, если у них RenderBelow = true
        for (int i = 0; i < _stack.Count - 1; i++)
        {
            if (_stack[i].RenderBelow)
                _stack[i].Render(_app);
        }

        Current!.Render(_app);
    }

    public void OnImGui()
    {
        if (_stack.Count == 0) return;

        // ImGui от нижних — только если рисуются
        for (int i = 0; i < _stack.Count - 1; i++)
        {
            if (_stack[i].RenderBelow)
                _stack[i].OnImGui(_app);
        }

        Current!.OnImGui(_app);
    }

    // ============================================================
    // Внутреннее
    // ============================================================

    private void ApplyPendingTransition()
    {
        if (_pending == null) return;
        var pending = _pending;
        _pending = null;

        if (pending == "pop")
        {
            DoPop();
            return;
        }

        var colon = pending.IndexOf(':');
        if (colon < 0) return;

        var op = pending[..colon];
        var name = pending[(colon + 1)..];

        if (op == "replace") DoReplace(name);
        else if (op == "push") DoPush(name);
    }

    private void DoReplace(string name)
    {
        // Выгружаем все сцены снизу вверх
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            try { _stack[i].OnUnload(_app); }
            catch (Exception ex) { Log.Error("SceneManager", $"OnUnload '{_stack[i].Name}': {ex.Message}"); }
        }
        _stack.Clear();

        var scene = Create(name);
        _stack.Add(scene);
        SafeLoad(scene);
    }

    private void DoPush(string name)
    {
        var scene = Create(name);
        _stack.Add(scene);
        SafeLoad(scene);
    }

    private void DoPop()
    {
        if (_stack.Count == 0) return;
        var top = _stack[^1];
        try { top.OnUnload(_app); }
        catch (Exception ex) { Log.Error("SceneManager", $"OnUnload '{top.Name}': {ex.Message}"); }
        _stack.RemoveAt(_stack.Count - 1);
    }

    private Scene Create(string name)
    {
        var scene = _registry[name]();
        scene.Name = name;
        return scene;
    }

    private void SafeLoad(Scene scene)
    {
        try { scene.OnLoad(_app); }
        catch (Exception ex)
        {
            Log.Error("SceneManager", $"OnLoad '{scene.Name}': {ex.Message}", ex);
            _stack.Remove(scene);
        }
    }

    private void EnsureRegistered(string name)
    {
        if (!_registry.ContainsKey(name))
            throw new InvalidOperationException(
                $"Scene '{name}' not registered. Available: {string.Join(", ", _registry.Keys)}");
    }
}