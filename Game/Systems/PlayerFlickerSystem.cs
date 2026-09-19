using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Game.Components;

namespace TestRPGGame.Systems;

/// <summary>
/// Мигание игрока при неуязвимости (i-frames).
/// Просто переключает Sprite.Enabled — остальное делает SpriteRenderSystem.
/// </summary>
public sealed class PlayerFlickerSystem : ISystem
{
    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 100; // после большинства логических систем

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<PlayerTag>())
        {
            var hp = e.Get<Health>();
            var sprite = e.Get<Sprite>();
            if (hp == null || sprite == null) continue;

            if (hp.InvulnTimer > 0)
            {
                // Мигаем: 10 герц
                bool visible = (int)(hp.InvulnTimer * 20) % 2 == 0;
                sprite.Enabled = visible;
            }
            else
            {
                sprite.Enabled = true;
            }
        }
    }
}