using MyEngine.Animation;
using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Events;

namespace MyEngine.Systems;

public sealed class AnimationSystem : ISystem
{
    private readonly EventBus? _events;

    public AnimationSystem(EventBus? events = null)
    {
        _events = events;
    }

    public void Update(World world, float dt)
    {
        foreach (var e in world.With<AnimatorComponent>())
        {
            var anim = e.Get<AnimatorComponent>()!;
            if (anim.Current == null || anim.Current.FrameCount == 0) continue;
            if (!anim.Playing) continue;

            float frameDuration = 1f / anim.Current.Fps;
            anim.Time += dt;

            int previousFrame = anim.FrameIndex;
            bool frameChanged = false;

            while (anim.Time >= frameDuration)
            {
                anim.Time -= frameDuration;
                anim.FrameIndex++;

                if (anim.FrameIndex >= anim.Current.FrameCount)
                {
                    if (anim.Current.Loop)
                    {
                        anim.FrameIndex = 0;
                    }
                    else
                    {
                        anim.FrameIndex = anim.Current.FrameCount - 1;
                        anim.Playing = false;
                        anim.OnComplete?.Invoke();
                        frameChanged = true;
                        break;
                    }
                }

                frameChanged = true;
            }

            // События на кадрах
            if (frameChanged && _events != null && anim.FrameIndex != previousFrame)
            {
                if (anim.Current.FrameEvents.TryGetValue(anim.FrameIndex, out var tag))
                {
                    _events.Publish(new AnimationEvent
                    {
                        Entity = e,
                        AnimationName = anim.Current.Name,
                        Tag = tag,
                        Frame = anim.FrameIndex
                    });
                }
            }

            // Прокидываем текущий кадр в Sprite
            var sprite = e.Get<Sprite>();
            if (sprite != null)
            {
                sprite.Texture = anim.Current.Texture;
                sprite.SourceRect = anim.Current.Frames[anim.FrameIndex];
            }
        }
    }
}