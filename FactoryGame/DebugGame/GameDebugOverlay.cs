using MyEngine.Diagnostics;
using MyEngine.Components;
using System.Numerics;
using MyEngine.Ecs;

namespace FactoryGame.DebugGame;

public sealed class GameDebugOverlay
{
    private readonly DebugDraw _dd;
    public GameDebugOverlay(DebugDraw dd) => _dd = dd;

    public void Draw(World world)
    {
        // 1. Сетка мира
        if (DebugConfig.ShowGrid)
        {
            var camCenter = new Vector2(0, 0); // отрисуем вокруг нуля; можно от камеры
            _dd.Grid(camCenter, extent: 2000, step: 100,
                new Vector4(0.25f, 0.28f, 0.32f, 0.5f));
        }

        // 2. Коллайдеры
        foreach (var e in world.With<Collider>())
        {
            var t = e.Get<Transform>();
            var c = e.Get<Collider>();
            if (t == null || c == null) continue;

            var color = c.IsStatic
                ? new Vector4(0.5f, 0.6f, 0.7f, 0.8f)   // стены — серо-синий
                : new Vector4(0.4f, 1f, 0.5f, 0.9f);    // динамика — зелёный
            _dd.Box(t.Position, c.Size, color);
        }


        // === ID над каждой сущностью ===
        // Рисуем поверх всего, для всех живых сущностей с Transform и Sprite.
        // Мелким серым текстом, чтобы не отвлекать.
        foreach (var e in world.With<Transform>())
        {
            var t = e.Get<Transform>()!;

            // Пропускаем сущности без спрайта — обычно это невидимые маркеры
            if (!e.Has<Sprite>()) continue;

            _dd.WorldText(
                t.Position + new Vector2(0, -34),   // выше центра сущности
                $"#{e.Id}",
                new Vector4(0.7f, 0.7f, 0.9f, 0.8f));
        }
    }
}