namespace MyEngine.Ecs;

/// <summary>
/// Расширения для запросов по сущностям. Дают fluent-синтаксис:
///
///   world.Query().With&lt;Transform&gt;().Without&lt;PlayerTag&gt;()
///   world.Query().With&lt;Transform&gt;().With&lt;Velocity&gt;()
///   world.Query().With&lt;A&gt;().With&lt;B&gt;().Without&lt;C&gt;()
///
/// Все методы — ленивые (yield return). Ничего не аллоцируется,
/// кроме итератора. Цепочка обходит список один раз.
/// </summary>
public static class EntityQueryExtensions
{
    /// <summary>Фильтр живых сущностей. Обычно первый в цепочке.</summary>
    public static IEnumerable<Entity> WhereAlive(this IEnumerable<Entity> source)
    {
        foreach (var e in source)
            if (e.IsAlive) yield return e;
    }

    /// <summary>Оставить только сущности с компонентом T.</summary>
    public static IEnumerable<Entity> With<T>(this IEnumerable<Entity> source)
        where T : class
    {
        foreach (var e in source)
            if (e.Has<T>()) yield return e;
    }

    /// <summary>Убрать сущности с компонентом T.</summary>
    public static IEnumerable<Entity> Without<T>(this IEnumerable<Entity> source)
        where T : class
    {
        foreach (var e in source)
            if (!e.Has<T>()) yield return e;
    }

    /// <summary>Оставить сущности, у которых есть ХОТЯ БЫ ОДИН из T1, T2.</summary>
    public static IEnumerable<Entity> WithAny<T1, T2>(this IEnumerable<Entity> source)
        where T1 : class where T2 : class
    {
        foreach (var e in source)
            if (e.Has<T1>() || e.Has<T2>()) yield return e;
    }

    /// <summary>Оставить сущности, у которых есть ХОТЯ БЫ ОДИН из трёх.</summary>
    public static IEnumerable<Entity> WithAny<T1, T2, T3>(this IEnumerable<Entity> source)
        where T1 : class where T2 : class where T3 : class
    {
        foreach (var e in source)
            if (e.Has<T1>() || e.Has<T2>() || e.Has<T3>()) yield return e;
    }
}