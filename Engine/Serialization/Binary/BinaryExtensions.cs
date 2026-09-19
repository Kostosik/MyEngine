using MyEngine.Math;
using System.Numerics;

namespace MyEngine.Serialization.Binary;

/// <summary>
/// Хелперы для записи/чтения стандартных типов.
/// </summary>
public static class BinaryExtensions
{
    public static void Write(this BinaryWriter w, Vector2 v)
    {
        w.Write(v.X);
        w.Write(v.Y);
    }

    public static Vector2 ReadVector2(this BinaryReader r)
        => new(r.ReadSingle(), r.ReadSingle());

    public static void Write(this BinaryWriter w, Vector4 v)
    {
        w.Write(v.X);
        w.Write(v.Y);
        w.Write(v.Z);
        w.Write(v.W);
    }

    public static Vector4 ReadVector4(this BinaryReader r)
        => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

    public static void Write(this BinaryWriter w, IntRect rect)
    {
        w.Write(rect.X);
        w.Write(rect.Y);
        w.Write(rect.Width);
        w.Write(rect.Height);
    }

    public static IntRect ReadIntRect(this BinaryReader r)
        => new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
}