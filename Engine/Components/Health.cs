using MyEngine.Diagnostics.Validation;
using System.Numerics;

namespace MyEngine.Components;

public sealed class Health
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
}