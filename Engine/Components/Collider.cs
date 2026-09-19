using MyEngine.Diagnostics.Validation;
using MyEngine.Physics;
using MyEngine.Serialization.Binary;
using System.Numerics;

namespace MyEngine.Components;

public sealed class Collider : IBinarySerializable
{
    public Vector2 Size;
    public bool IsStatic;

    /// <summary>К какому слою относится этот объект.</summary>
    public Layer Layer = Layer.None;

    /// <summary>С какими слоями этот объект хочет сталкиваться.</summary>
    public Layer CollidesWith = Layer.None;

    /// <summary>
    /// Если true — коллайдер не разрешает столкновения, а только
    /// генерирует события TriggerEnter/TriggerExit. См. TriggerSystem.
    /// </summary>
    public bool IsTrigger = false;

    public void Write(BinaryWriter w)
    {
        w.Write(Size);
        w.Write(IsStatic);
    }

    public void Read(BinaryReader w)
    {
        Size = w.ReadVector2();
        IsStatic = w.ReadBoolean();
    }
}