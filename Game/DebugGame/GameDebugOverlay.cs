using MyEngine.Diagnostics;
using MyEngine.Game.Components;
using MyEngine.Components;
using System.Numerics;
using MyEngine.Ecs;

namespace MyEngine.Game.Debug;

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

        // 3. Радиусы AI
        foreach (var e in world.With<AI>())
        {
            var t = e.Get<Transform>();
            var ai = e.Get<AI>();
            if (t == null || ai == null) continue;

            _dd.Circle(t.Position, ai.AggroRadius, new Vector4(1f, 0.9f, 0.3f, 0.35f));
            _dd.Circle(t.Position, ai.AttackRange, new Vector4(1f, 0.5f, 0.2f, 0.5f));

            // Дом
            _dd.Circle(ai.HomePosition, 4f, new Vector4(0.9f, 0.9f, 0.9f, 1f), 8);

            // Состояние над головой
            _dd.WorldText(t.Position + new Vector2(-20, -30),
                ai.State.ToString(), new Vector4(1f, 1f, 1f, 0.9f));
        }

        // 4. Векторы скорости
        foreach (var e in world.With<Velocity>())
        {
            var t = e.Get<Transform>();
            var v = e.Get<Velocity>();
            if (t == null || v == null) continue;
            if (v.Value.LengthSquared() < 1f) continue;

            var tip = t.Position + v.Value * 0.3f;
            _dd.Arrow(t.Position, tip, new Vector4(0.4f, 0.7f, 1f, 0.9f));
        }

        // 5. HP над врагами
        foreach (var e in world.With<EnemyTag>())
        {
            var t = e.Get<Transform>();
            var hp = e.Get<Health>();
            if (t == null || hp == null) continue;

            // Маленькая полоска над головой
            float w = 40f, h = 4f;
            float frac = hp.MaxHp > 0 ? (float)hp.Hp / hp.MaxHp : 0;
            var bg = t.Position + new Vector2(0, -40);
            _dd.Box(bg, new Vector2(w, h), new Vector4(0f, 0f, 0f, 0.7f), filled: true);
            var fg = t.Position + new Vector2(-w * 0.5f + w * frac * 0.5f, -40);
            _dd.Box(fg, new Vector2(w * frac, h), new Vector4(0.9f, 0.3f, 0.3f, 0.9f), filled: true);
        }

        // 6. Хитбокс игрока при ударе
        foreach (var e in world.With<PlayerTag>())
        {
            var t = e.Get<Transform>();
            var a = e.Get<Attack>();
            if (t == null || a == null) continue;

            if (a.ActiveTimer > 0)
                _dd.Circle(t.Position, a.Range, new Vector4(1f, 1f, 0.5f, 0.7f), 24);
        }

        // === ID над каждой сущностью ===
        // Рисуем поверх всего, для всех живых сущностей с Transform и Sprite.
        // Мелким серым текстом, чтобы не отвлекать.
        foreach (var e in world.With<Transform>())
        {
            var t = e.Get<Transform>()!;

            // Пропускаем сущности без спрайта — обычно это невидимые маркеры
            if (!e.Has<MyEngine.Components.Sprite>()) continue;

            _dd.WorldText(
                t.Position + new Vector2(0, -34),   // выше центра сущности
                $"#{e.Id}",
                new Vector4(0.7f, 0.7f, 0.9f, 0.8f));
        }
    }
}