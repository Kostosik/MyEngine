using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Events;
using MyEngine.InputEngine;
using System.Numerics;

namespace MyEngine.Systems;

/// <summary>
/// Система взаимодействия. Для каждой сущности с Interactor ищет
/// ближайший Interactable в радиусе и сохраняет его в Interactor.CurrentTarget.
///
/// При нажатии действия Interact публикует InteractionRequestedEvent.
/// Игра решает, что делать — открыть диалог, активировать маяк, подобрать предмет.
/// </summary>
public sealed class InteractionSystem : ISystem
{
    private readonly Input _input;
    private readonly EventBus _events;

    public InteractionSystem(Input input, EventBus events)
    {
        _input = input;
        _events = events;
    }

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<Interactor>())
        {
            var interactor = e.Get<Interactor>()!;
            var t = e.Get<Transform>();
            if (t == null)
            {
                interactor.CurrentTarget = null;
                continue;
            }

            Entity? best = null;
            float bestDist = float.MaxValue;

            foreach (var target in world.With<Interactable>())
            {
                if (ReferenceEquals(target, e)) continue;

                var tt = target.Get<Transform>();
                var i = target.Get<Interactable>()!;
                if (tt == null) continue;

                float d = Vector2.Distance(t.Position, tt.Position);
                if (d > i.Radius) continue;
                if (d < bestDist) { bestDist = d; best = target; }
            }

            interactor.CurrentTarget = best;

            // Нажатие "Interact" — публикуем событие
            if (best != null && _input.ConsumeActionPressed(GameAction.Interact))
            {
                _events.Publish(new InteractionRequestedEvent
                {
                    Interactor = e,
                    Target = best
                });
            }
        }
    }
}