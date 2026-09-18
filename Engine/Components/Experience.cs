using MyEngine.Diagnostics.Validation;

namespace MyEngine.Components;

public sealed class Experience
{
    [Positive]
    public int Reward; // сколько XP даёт при смерти
}