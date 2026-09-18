using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Рисует все сущности с Transform + Sprite. Сортирует по Sprite.Layer
/// (меньше — раньше, «сзади»). Пропускает спрайты с Enabled = false.
///
/// Не знает про игровую логику: HP, атаки, теги. Игровые визуальные
/// эффекты реализуются системами в игре — они меняют Color и Enabled.
/// </summary>
public sealed class SpriteRenderSystem
{
    private readonly SpriteBatch _batch;
    private readonly List<Entity> _sortBuffer = new();

    public SpriteRenderSystem(SpriteBatch batch) => _batch = batch;

    /// <summary>Вызывать внутри batch.Begin/End.</summary>
    public void Draw(World world)
    {
        // Собрать и отсортировать по Layer
        _sortBuffer.Clear();
        foreach (var e in world.With<Sprite>())
            _sortBuffer.Add(e);

        _sortBuffer.Sort((a, b) =>
            a.Get<Sprite>()!.Layer.CompareTo(b.Get<Sprite>()!.Layer));

        foreach (var e in _sortBuffer)
        {
            var t = e.Get<Transform>();
            var s = e.Get<Sprite>()!;
            if (t == null) continue;
            if (!s.Enabled) continue;

            // С текстурой + регионом — рисуем регион
            if (s.Texture != null && s.SourceRect.HasValue)
            {
                var r = s.SourceRect.Value;
                float u0 = r.X / (float)s.Texture.Width;
                float v0 = r.Y / (float)s.Texture.Height;
                float u1 = r.Right / (float)s.Texture.Width;
                float v1 = r.Bottom / (float)s.Texture.Height;

                _batch.Draw(
                    s.Texture, t.Position, s.Size,
                    new Vector2(u0, v0), new Vector2(u1, v1),
                    0f, null, s.Color,
                    s.EffectType,        // ← NEW
                    s.EffectParams);     // ← NEW
            }
            // С текстурой без региона — целиком
            else if (s.Texture != null)
            {
                _batch.Draw(s.Texture, t.Position, s.Size, 0f, null, s.Color);
            }
            // Цветной прямоугольник
            else
            {
                _batch.DrawRect(t.Position, s.Size, s.Color);
            }
        }
    }
}