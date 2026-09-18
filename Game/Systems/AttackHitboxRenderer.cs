using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Game.Components;
using MyEngine.Rendering;
using System.Numerics;

namespace MyEngine.Game.Systems;

/// <summary>
/// Рисует хитбокс игрока при ударе. Вызывается в Render после SpriteRenderSystem.
/// Игрок устанавливается отдельно — после создания сущности.
/// </summary>
public sealed class AttackHitboxRenderer
{
    private readonly SpriteBatch _batch;
    private Entity? _player;

    public AttackHitboxRenderer(SpriteBatch batch) => _batch = batch;

    /// <summary>Установить игрока. Вызывается из FinishLoading после создания сущности.</summary>
    public void SetPlayer(Entity player) => _player = player;

    public void Draw()
    {
        if (_player == null) return;

        var attack = _player.Get<Attack>();
        var t = _player.Get<Transform>();
        if (attack == null || t == null) return;
        if (attack.ActiveTimer <= 0) return;

        float r = attack.Range;
        var hitColor = new Vector4(1f, 1f, 0.6f, 0.25f);
        _batch.DrawRect(t.Position, new Vector2(r * 2, r * 2), hitColor);
    }
}