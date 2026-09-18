using MyEngine.Diagnostics.Validation;

namespace MyEngine.Components;

public sealed class Attack
{
    [Positive]
    public int Damage;
    [Positive]
    public float Range;// радиус удара
    [Positive]
    public float Cooldown;     // сколько ждать между ударами
    public float TimeSinceLast; // внутреннее
    public float ActiveTimer;  // сколько ещё показывать хитбокс визуально
    public bool IsReady => TimeSinceLast >= Cooldown;
}