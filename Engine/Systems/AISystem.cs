using MyEngine.Ecs;

namespace MyEngine.Systems;

public sealed class AISystem : ISystem
{

    public SystemPhase Phase => SystemPhase.Update;
    public int Priority => 0;
    public void Update(World world, float dt)
    {
        foreach (var e in world.With<BehaviorComponent>())
        {
            var bc = e.Get<BehaviorComponent>()!;

            if (!bc.Started)
            {
                bc.Behavior.Tick(e, world, 0f); // прогрев
                bc.Started = true;
            }

            bc.Behavior.Tick(e, world, dt);
        }
    }
}