using MyEngine.Ecs;
using MyEngine.Game.Components;
using MyEngine.Components;
using System.Numerics;

namespace MyEngine.Game.Systems;

public sealed class PickupSystem : ISystem
{
    private readonly Entity _player;
    private readonly GameState _state;
    private readonly DamagePopupSystem _popups;

    private const float PickupRadius = 28f;

    public PickupSystem(Entity player, GameState state, DamagePopupSystem popups)
    {
        _player = player;
        _state = state;
        _popups = popups;
    }

    public void Update(World world, float dt)
    {
        var pt = _player.Get<Transform>();
        if (pt == null) return;

        foreach (var e in world.With<Pickup>())
        {
            var pickup = e.Get<Pickup>()!;
            if (pickup.Collected) continue;

            var t = e.Get<Transform>();
            if (t == null) continue;

            float dist = Vector2.Distance(pt.Position, t.Position);
            if (dist > PickupRadius) continue;

            pickup.Collected = true;

            if (pickup.Kind == "spark")
            {
                _state.SparksCollected++;

                // Всплывашка "Spark N / M"
                _popups.SpawnText(t.Position,
                    $"Spark {_state.SparksCollected} / {_state.SparksTotal}",
                    new Vector4(1f, 0.95f, 0.4f, 1f));
            }

            world.Destroy(e);
        }
    }
}