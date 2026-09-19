using MyEngine.Diagnostics.Validation;
using MyEngine.Serialization.Binary;
using System.Numerics;

namespace MyEngine.Components;

public sealed class Health : IBinarySerializable
{
    [NonNegative]
    public int Hp;

    [Positive]
    public int MaxHp;
    public float InvulnTimer;
    public float HitFlashTimer;

    /// <summary>Базовый цвет спрайта — для восстановления после вспышки.</summary>
    public Vector4 BaseColor = Vector4.One;

    public bool Invulnerable;
    public bool IsAlive => Hp > 0;

    public void Write(BinaryWriter w)
    {
        w.Write(Hp);
        w.Write(MaxHp);
        w.Write(InvulnTimer);
        w.Write(HitFlashTimer);
        w.Write(BaseColor);
        w.Write(Invulnerable);
    }

    public void Read(BinaryReader r)
    {
        Hp = r.ReadInt32();
        MaxHp = r.ReadInt32();
        InvulnTimer = r.ReadSingle();
        HitFlashTimer = r.ReadSingle();
        BaseColor = r.ReadVector4();
        Invulnerable = r.ReadBoolean();
    }
}