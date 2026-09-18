using MyEngine.Effects;
using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.Effects;

/// <summary>
/// Рендер частиц. Просто проходит по пулу и рисует активные
/// как цветные квадраты с интерполяцией размера и цвета по прогрессу жизни.
/// </summary>
public static class ParticleRenderer
{
    public static void Draw(SpriteBatch batch, ParticleSystem system)
    {
        var span = system.Pool.Particles;
        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var p = ref span[i];
            if (!p.Active) continue;

            float t = 1f - (p.Life / p.MaxLife); // 0 в начале, 1 в конце

            var size = Vector2.Lerp(p.SizeStart, p.SizeEnd, t);
            var color = Vector4.Lerp(p.ColorStart, p.ColorEnd, t);

            batch.DrawRect(p.Position, size, color);
        }
    }
}