namespace MyEngine.Animation;

/// <summary>
/// Состояние проигрывания анимации на сущности.
/// Хранит текущую анимацию, время, индекс кадра.
/// </summary>
public sealed class AnimatorComponent
{
    /// <summary>Текущая анимация. Null — ничего не играется.</summary>
    public AnimationEngine? Current;

    /// <summary>Время с момента начала текущего кадра.</summary>
    public float Time;

    /// <summary>Индекс текущего кадра.</summary>
    public int FrameIndex;

    /// <summary>Играется ли анимация. Если false — время не идёт, кадр не меняется.</summary>
    public bool Playing = true;

    /// <summary>Вызывается, когда не-циклическая анимация доиграла до конца.</summary>
    public Action? OnComplete;

    /// <summary>
    /// Запустить анимацию.
    /// </summary>
    /// <param name="restart">Если true — сбросить время и начать с нулевого кадра,
    /// даже если эта анимация уже играет. Если false — повторный Play той же
    /// анимации ничего не меняет (удобно для спама "Play(walk)" каждый кадр).</param>
    public void Play(AnimationEngine anim, bool restart = false)
    {
        if (Current == anim && !restart) return;
        Current = anim;
        Time = 0;
        FrameIndex = 0;
        Playing = true;
    }

    public void Stop()
    {
        Playing = false;
    }

    public void Resume()
    {
        Playing = true;
    }
}