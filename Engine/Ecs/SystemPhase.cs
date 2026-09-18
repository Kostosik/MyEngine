namespace MyEngine.Ecs;

/// <summary>
/// Фаза выполнения системы. Движок тикает фазы в фиксированном порядке:
/// PreUpdate → Update → PostUpdate → LateUpdate.
///
/// Зачем: когда систем 20+, линейный список становится хрупким —
/// одна вставка в середине ломает всю логику. Фазы дают явный порядок.
/// </summary>
public enum SystemPhase
{
    /// <summary>Раньше всего: сбор ввода, подготовка данных.</summary>
    PreUpdate,

    /// <summary>Основная логика: AI, движение, физика, бой.</summary>
    Update,

    /// <summary>После основной логики: разрешение коллизий, реакции.</summary>
    PostUpdate,

    /// <summary>В самом конце: камера, звук, анимация на основе финального состояния.</summary>
    LateUpdate
}