using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Rendering;

namespace MyEngine.Systems;

/// <summary>
/// Обновляет позиции объектов с ParallaxLayer в зависимости
/// от позиции камеры. Обычно вызывается в LateUpdate, чтобы
/// использовать финальную позицию камеры этого кадра.
/// </summary>
public sealed class ParallaxSystem : ISystem
{
    private readonly Camera2D _camera;

    public ParallaxSystem(Camera2D camera) => _camera = camera;

    public SystemPhase Phase => SystemPhase.LateUpdate;
    public int Priority => -10; // раньше других LateUpdate-систем

    public void Update(World world, float dt)
    {
        var camPos = _camera.Position;

        foreach (var e in world.With<ParallaxLayer>())
        {
            var layer = e.Get<ParallaxLayer>()!;
            var t = e.Get<Transform>();
            if (t == null) continue;

            // Формула: сдвигаем объект на позицию камеры, масштабированную
            // на (1 - factor). При factor=1 — не сдвигаем (движется с миром).
            // При factor=0 — сдвигаем полностью (стоит на экране).
            t.Position = layer.BasePosition
                       + camPos * (1f - layer.ParallaxFactor)
                       + layer.Offset;
        }
    }
}