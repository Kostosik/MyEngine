using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.InputEngine;

namespace MyEngine.Systems;

/// <summary>
/// Читает Bindings и пишет в PlatformerController.
/// Отдельно от PlayerInputSystem (который пишет в TopDownController) —
/// чтобы движок не зависел от конкретной игры.
/// </summary>
public sealed class PlatformerInputSystem : ISystem
{
    private readonly Input _input;
    private readonly float _speedMultiplier;

    public PlatformerInputSystem(Input input, float speedMultiplier = 1f)
    {
        _input = input;
        _speedMultiplier = speedMultiplier;
    }

    public SystemPhase Phase => SystemPhase.PreUpdate;

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<PlatformerController>())
        {
            var c = e.Get<PlatformerController>()!;

            float x = 0f;
            if (_input.IsActionDown(GameAction.MoveLeft)) x -= 1f;
            if (_input.IsActionDown(GameAction.MoveRight)) x += 1f;

            c.InputX = x * _speedMultiplier;
            c.JumpHeld = _input.IsActionDown(GameAction.Jump);

            if (_input.WasActionPressed(GameAction.Jump))
                c.JumpPressed = true;
        }
    }
}