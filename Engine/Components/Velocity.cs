using MyEngine.Serialization.Binary;
using System.Numerics;

namespace MyEngine.Components;

public sealed class Velocity : IBinarySerializable
{
    public Vector2 Value;

    public void Write(BinaryWriter w) => w.Write(Value);
    public void Read(BinaryReader r) => Value = r.ReadVector2();
}