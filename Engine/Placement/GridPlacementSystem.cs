using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.InputEngine;
using MyEngine.Rendering;
using MyEngine.Tilemap;
using System.Numerics;

namespace MyEngine.Placement;

/// <summary>
/// Ставит здания по сетке тайлов мышью.
///
/// Управление:
///   ЛКМ — поставить
///   ПКМ — удалить под курсором
///   R   — повернуть (90°)
///   Esc — отменить
///
/// Не зависит от конкретной игры — принимает Input, Camera2D и TilemapComponent.
/// </summary>
public sealed class GridPlacementSystem : ISystem
{
    private readonly Input _input;
    private readonly Camera2D _camera;
    private readonly EntityFactoryRegistry _factories;

    private TilemapComponent? _tilemap;
    private Entity? _ghost;

    /// <summary>Тип здания, которое ставим сейчас. Null — режим выключен.</summary>
    public string? ActiveTypeId { get; private set; }

    /// <summary>Текущий поворот (0..3).</summary>
    public int Rotation { get; private set; }

    public GridPlacementSystem(
        Input input,
        Camera2D camera,
        EntityFactoryRegistry factories)
    {
        _input = input;
        _camera = camera;
        _factories = factories;
    }

    /// <summary>Установить карту. Вызывать после загрузки Tilemap.</summary>
    public void SetTilemap(TilemapComponent tilemap) => _tilemap = tilemap;

    public void BeginPlacement(string typeId)
    {
        ActiveTypeId = typeId;
        Rotation = 0;
        // Призрак создаётся в первом Update — там есть World
    }

    public void CancelPlacement()
    {
        ActiveTypeId = null;
        _ghostPendingDestroy = true;
    }

    private bool _ghostPendingDestroy;

    public void Update(World world, float dt)
    {
        if (_ghostPendingDestroy)
        {
            DestroyGhost(world);
            _ghostPendingDestroy = false;
        }

        // Хоткеи — до любых проверок
        if (_input.ConsumePressed(Silk.NET.Input.Key.Escape))
            CancelPlacement();

        if (ActiveTypeId == null || _tilemap == null)
        {
            DestroyGhost(world);
            return;
        }

        // Создаём призрак, если нет
        if (_ghost == null)
            CreateGhost(world);

        if (_input.ConsumePressed(Silk.NET.Input.Key.R))
        {
            Rotation = (Rotation + 1) % 4;
            ApplyRotation(_ghost);
        }

        // Мышь → тайл
        var mouseWorld = _camera.ScreenToWorld(
            _input.MousePosition, _camera.ScreenWidth, _camera.ScreenHeight);
        // ^ ScreenWidth/Height — если их нет, передавай в конструктор
        var (tileX, tileY) = _tilemap.WorldToTile(mouseWorld);
        var worldPos = TileCenterWorld(tileX, tileY);

        // Позиция призрака
        UpdateGhostPosition(_ghost, worldPos);

        // ЛКМ — поставить
        if (_input.ConsumeMousePressed(Silk.NET.Input.MouseButton.Left))
            TryPlace(world, tileX, tileY, worldPos);

        // ПКМ — удалить
        if (_input.ConsumeMousePressed(Silk.NET.Input.MouseButton.Right))
            TryDelete(world, tileX, tileY);
    }

    // ============================================================
    // Призрак
    // ============================================================

    private void CreateGhost(World world)
    {
        if (ActiveTypeId == null) return;

        _ghost = _factories.Spawn(world, ActiveTypeId, Vector2.Zero);
        _ghost.Remove<Collider>();
        _ghost.Remove<PlaceableComponent>();  // призрак не занимает клетку

        var sprite = _ghost.Get<Sprite>();
        if (sprite != null)
            sprite.Color = new Vector4(sprite.Color.X, sprite.Color.Y, sprite.Color.Z, 0.5f);

        ApplyRotation(_ghost);
    }

    private void DestroyGhost(World world)
    {
        if (_ghost == null) return;
        world.Destroy(_ghost);
        _ghost = null;
    }

    private static void UpdateGhostPosition(Entity ghost, Vector2 pos)
    {
        var t = ghost.Get<Transform>();
        if (t != null) t.Position = pos;
    }

    private void ApplyRotation(Entity ghost)
    {
        var t = ghost.Get<Transform>();
        if (t != null) t.Rotation = Rotation * MathF.PI / 2f;
    }

    // ============================================================
    // Действия
    // ============================================================

    private void TryPlace(World world, int tileX, int tileY, Vector2 worldPos)
    {
        if (ActiveTypeId == null || _tilemap == null) return;

        if (!CanPlaceAt(world, tileX, tileY))
        {
            Log.Warn("Placement", $"Cannot place at ({tileX}, {tileY})");
            return;
        }

        var placed = _factories.Spawn(world, ActiveTypeId, worldPos);
        placed.Add(new PlaceableComponent { TypeId = ActiveTypeId });
        ApplyRotation(placed);
    }

    private void TryDelete(World world, int tileX, int tileY)
    {
        if (_tilemap == null) return;

        foreach (var e in world.Query().With<PlaceableComponent>().With<Transform>())
        {
            var t = e.Get<Transform>()!;
            var (ex, ey) = _tilemap.WorldToTile(t.Position);
            if (ex == tileX && ey == tileY)
            {
                world.Destroy(e);
                return;
            }
        }
    }

    // ============================================================
    // Проверки
    // ============================================================

    private bool CanPlaceAt(World world, int tileX, int tileY)
    {
        if (_tilemap == null) return false;

        // Границы
        if (tileX < 0 || tileY < 0 || tileX >= _tilemap.Width || tileY >= _tilemap.Height)
            return false;

        // Занято
        foreach (var e in world.Query().With<PlaceableComponent>().With<Transform>())
        {
            var t = e.Get<Transform>()!;
            var (ex, ey) = _tilemap.WorldToTile(t.Position);
            if (ex == tileX && ey == tileY) return false;
        }

        return true;
    }

    // ============================================================
    // Координаты
    // ============================================================

    private Vector2 TileCenterWorld(int tileX, int tileY)
    {
        if (_tilemap == null) return Vector2.Zero;

        int ts = _tilemap.TileSize;
        return _tilemap.Origin + new Vector2(
            tileX * ts + ts * 0.5f,
            tileY * ts + ts * 0.5f);
    }
}