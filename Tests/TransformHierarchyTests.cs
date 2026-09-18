using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Systems;
using System.Numerics;

namespace MyEngine.Tests;

public class TransformHierarchyTests
{
    private static (World world, Transform t) MakeWorldWithTransform()
    {
        var world = new World();
        var e = world.Create();
        var t = new Transform();
        e.Add(t);
        return (world, t);
    }

    private static void Tick(World world)
    {
        new TransformSystem().Update(world, 1f / 60f);
    }

    // ============================================================
    // Базовая привязка
    // ============================================================

    [Fact]
    public void NoParent_DoesNotChangePosition()
    {
        var (world, t) = MakeWorldWithTransform();
        t.Position = new Vector2(100, 200);

        Tick(world);

        Assert.Equal(100f, t.Position.X);
        Assert.Equal(200f, t.Position.Y);
    }

    [Fact]
    public void Child_FollowsParentPosition()
    {
        var (world, parent) = MakeWorldWithTransform();
        var (_, child) = MakeWorldWithTransform();
        // Оба в одном world — используем один
        // Проще: создать второй Transform в том же world
        world = new World();
        var parentEntity = world.Create();
        var childEntity = world.Create();
        parent = new Transform();
        child = new Transform();
        parentEntity.Add(parent);
        childEntity.Add(child);

        parent.Position = new Vector2(100, 0);
        child.LocalPosition = new Vector2(10, 5);
        child.SetParent(parent, worldPositionStays: false);

        Tick(world);

        Assert.Equal(110f, child.Position.X);
        Assert.Equal(5f, child.Position.Y);
    }

    [Fact]
    public void ParentMoves_ChildFollows()
    {
        var world = new World();
        var pe = world.Create();
        var ce = world.Create();
        var parent = new Transform();
        var child = new Transform();
        pe.Add(parent);
        ce.Add(child);

        child.LocalPosition = new Vector2(20, 0);
        child.SetParent(parent, false);

        parent.Position = new Vector2(100, 0);
        Tick(world);
        Assert.Equal(120f, child.Position.X);

        parent.Position = new Vector2(200, 0);
        Tick(world);
        Assert.Equal(220f, child.Position.X);
    }

    // ============================================================
    // Вложенность
    // ============================================================

    [Fact]
    public void Grandchild_FollowsChain()
    {
        var world = new World();
        var p = new Transform();
        var c = new Transform();
        var g = new Transform();

        world.Create().Add(p);
        world.Create().Add(c);
        world.Create().Add(g);

        c.LocalPosition = new Vector2(10, 0);
        c.SetParent(p, false);

        g.LocalPosition = new Vector2(5, 0);
        g.SetParent(c, false);

        p.Position = new Vector2(100, 0);
        Tick(world);

        Assert.Equal(110f, c.Position.X);
        Assert.Equal(115f, g.Position.X);
    }

    // ============================================================
    // SetParent с worldPositionStays
    // ============================================================

    [Fact]
    public void SetParent_WorldPositionStays_ChildDoesNotMove()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 100) };
        var c = new Transform { Position = new Vector2(150, 120) };

        world.Create().Add(p);
        world.Create().Add(c);

        // Привязываем, сохраняя мировую позицию
        c.SetParent(p, worldPositionStays: true);

        // Локальная позиция должна быть (50, 20)
        Assert.Equal(50f, c.LocalPosition.X);
        Assert.Equal(20f, c.LocalPosition.Y);

        Tick(world);
        // Мировая не изменилась
        Assert.Equal(150f, c.Position.X);
        Assert.Equal(120f, c.Position.Y);
    }

    [Fact]
    public void SetParent_NoWorldStays_ChildJumps()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 100) };
        var c = new Transform { Position = new Vector2(150, 120) };
        world.Create().Add(p);
        world.Create().Add(c);

        // LocalPosition по умолчанию (0, 0) — ребёнок «прыгнет» к родителю
        c.SetParent(p, worldPositionStays: false);

        Tick(world);
        Assert.Equal(100f, c.Position.X);
        Assert.Equal(100f, c.Position.Y);
    }

    // ============================================================
    // Отвязка
    // ============================================================

    [Fact]
    public void DetachFromParent_StaysAtPosition()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 0) };
        var c = new Transform();
        world.Create().Add(p);
        world.Create().Add(c);

        c.LocalPosition = new Vector2(20, 0);
        c.SetParent(p, false);
        Tick(world);
        Assert.Equal(120f, c.Position.X);

        c.DetachFromParent();

        // Двигаем родителя — ребёнок не должен следовать
        p.Position = new Vector2(500, 0);
        Tick(world);

        Assert.Equal(120f, c.Position.X);
        Assert.Null(c.Parent);
    }

    [Fact]
    public void DetachChildren_AllBecomeIndependent()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 0) };
        var c1 = new Transform();
        var c2 = new Transform();
        world.Create().Add(p);
        world.Create().Add(c1);
        world.Create().Add(c2);

        c1.SetParent(p, false);
        c2.SetParent(p, false);

        Assert.Equal(2, p.Children.Count);

        p.DetachChildren();

        Assert.Empty(p.Children);
        Assert.Null(c1.Parent);
        Assert.Null(c2.Parent);
    }

    // ============================================================
    // Прочие свойства
    // ============================================================

    [Fact]
    public void SetWorldPosition_UpdatesLocal()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 0) };
        var c = new Transform();
        world.Create().Add(p);
        world.Create().Add(c);

        c.SetParent(p, false);
        c.SetWorldPosition(new Vector2(150, 50));

        Assert.Equal(50f, c.LocalPosition.X);
        Assert.Equal(50f, c.LocalPosition.Y);
    }

    [Fact]
    public void SetLocalPosition_UpdatesWorld()
    {
        var world = new World();
        var p = new Transform { Position = new Vector2(100, 0) };
        var c = new Transform();
        world.Create().Add(p);
        world.Create().Add(c);

        c.SetParent(p, false);
        c.SetLocalPosition(new Vector2(30, 40));

        Assert.Equal(130f, c.Position.X);
        Assert.Equal(40f, c.Position.Y);
    }

    [Fact]
    public void RotationAndScale_Propagate()
    {
        var world = new World();
        var p = new Transform { Position = Vector2.Zero, Rotation = 1f, Scale = new Vector2(2, 2) };
        var c = new Transform();
        world.Create().Add(p);
        world.Create().Add(c);

        c.LocalRotation = 0.5f;
        c.LocalScale = new Vector2(0.5f, 0.5f);
        c.SetParent(p, false);

        Tick(world);

        Assert.Equal(1.5f, c.Rotation, 3);
        Assert.Equal(1f, c.Scale.X, 3);
        Assert.Equal(1f, c.Scale.Y, 3);
    }

    [Fact]
    public void ReparentToAnother_Works()
    {
        var world = new World();
        var p1 = new Transform { Position = new Vector2(100, 0) };
        var p2 = new Transform { Position = new Vector2(0, 100) };
        var c = new Transform();
        world.Create().Add(p1);
        world.Create().Add(p2);
        world.Create().Add(c);

        c.SetParent(p1, false);
        Tick(world);

        c.SetParent(p2, false);
        Tick(world);

        Assert.Same(p2, c.Parent);
        Assert.DoesNotContain(c, p1.Children);
        Assert.Contains(c, p2.Children);
    }
}