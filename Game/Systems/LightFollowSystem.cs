using MyEngine.Components;
using MyEngine.Ecs;

namespace MyEngine.Game.Systems;

/// <summary>Свет, следующий за игроком.</summary>
public sealed class LightFollowSystem : ISystem
{
    private readonly Entity _player;
    private readonly Entity _light;

    public LightFollowSystem(Entity player, Entity light)
    {
        _player = player;
        _light = light;
    }

    public SystemPhase Phase => SystemPhase.LateUpdate;

    public void Update(World world, float dt)
    {
        var p = _player.Get<Transform>();
        var l = _light.Get<Transform>();
        if (p == null || l == null) return;
        l.Position = p.Position;
    }
}