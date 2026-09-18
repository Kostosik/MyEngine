using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Game.Components;
using MyEngine.Rendering.Effects;
using System.Numerics;

namespace MyEngine.Game.Systems;

/// <summary>
/// Вспышка белым при получении урона. Меняет Sprite.Color
/// временно к белому, потом возвращает к базовому.
///
/// Базовый цвет нужно запомнить — храним в самом компоненте.
/// </summary>
public sealed class HitFlashSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 100;

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<Health>())
        {
            var hp = e.Get<Health>()!;
            var sprite = e.Get<Sprite>();
            if (sprite == null) continue;

            if (hp.HitFlashTimer > 0)
            {
                sprite.EffectType = SpriteEffect.Flash;
                sprite.EffectParams = new Vector4(0.8f, 1f, 1f, 1f);  // сила, цвет RGB
            }
            else
            {
                sprite.EffectType = SpriteEffect.None;
            }
        }
    }
}