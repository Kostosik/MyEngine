using MyEngine.Components;
using MyEngine.Ecs;
using MyEngine.Serialization.Binary;
using System.Numerics;

namespace MyEngine.Tests;

public class BinarySerializerTests
{
    private class TestComp : IBinarySerializable
    {
        public int Value;
        public Vector2 Pos;

        public void Write(BinaryWriter w)
        {
            w.Write(Value);
            w.Write(Pos);
        }

        public void Read(BinaryReader r)
        {
            Value = r.ReadInt32();
            Pos = r.ReadVector2();
        }
    }

    [Fact]
    public void RoundTrip_EmptyWorld()
    {
        var world = new World();
        var registry = new BinaryComponentRegistry();
        registry.Register(() => new TestComp());
        var ser = new BinaryWorldSerializer(registry);

        var bytes = ser.SaveToBytes(world);

        var world2 = new World();
        ser.LoadFromBytes(world2, bytes);

        Assert.Empty(world2.Entities);
    }

    [Fact]
    public void RoundTrip_OneEntity()
    {
        var world = new World();
        var e = world.Create();
        e.Add(new TestComp { Value = 42, Pos = new Vector2(1.5f, 2.5f) });

        var registry = new BinaryComponentRegistry();
        registry.Register(() => new TestComp());
        var ser = new BinaryWorldSerializer(registry);

        var bytes = ser.SaveToBytes(world);

        var world2 = new World();
        ser.LoadFromBytes(world2, bytes);

        var loaded = world2.Entities.First();
        var comp = loaded.Get<TestComp>()!;
        Assert.Equal(42, comp.Value);
        Assert.Equal(1.5f, comp.Pos.X);
        Assert.Equal(2.5f, comp.Pos.Y);
    }

    [Fact]
    public void RoundTrip_PreservesEntityId()
    {
        var world = new World();
        var e = world.Create();
        e.Add(new TestComp { Value = 7 });
        int originalId = e.Id;

        var registry = new BinaryComponentRegistry();
        registry.Register(() => new TestComp());
        var ser = new BinaryWorldSerializer(registry);

        var bytes = ser.SaveToBytes(world);

        var world2 = new World();
        ser.LoadFromBytes(world2, bytes);

        var loaded = world2.GetById(originalId);
        Assert.NotNull(loaded);
    }

    [Fact]
    public void RoundTrip_MultipleComponents()
    {
        var world = new World();
        var e = world.Create();
        e.Add(new Transform { Position = new Vector2(100, 200), Rotation = 1.5f });
        e.Add(new Velocity { Value = new Vector2(10, -5) });

        var registry = new BinaryComponentRegistry();
        registry.Register(() => new Transform());
        registry.Register(() => new Velocity());
        var ser = new BinaryWorldSerializer(registry);

        var bytes = ser.SaveToBytes(world);

        var world2 = new World();
        ser.LoadFromBytes(world2, bytes);

        var loaded = world2.Entities.First();
        Assert.Equal(new Vector2(100, 200), loaded.Get<Transform>()!.Position);
        Assert.Equal(1.5f, loaded.Get<Transform>()!.Rotation);
        Assert.Equal(new Vector2(10, -5), loaded.Get<Velocity>()!.Value);
    }

    [Fact]
    public void RoundTrip_InvalidMagic_Throws()
    {
        var world = new World();
        var bytes = new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0 };
        var registry = new BinaryComponentRegistry();
        var ser = new BinaryWorldSerializer(registry);

        Assert.Throws<InvalidDataException>(() => ser.LoadFromBytes(world, bytes));
    }
}