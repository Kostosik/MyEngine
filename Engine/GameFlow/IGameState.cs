namespace MyEngine.GameFlow;

/// <summary>
/// Состояние игры (Loading, Playing, Dialogue, Dead, Victory, ...).
/// Каждое состояние — это фаза, в которой игра может находиться.
/// Управляет переходами через GameStateMachine.
/// </summary>
public interface IGameState
{
    /// <summary>Вызывается один раз при входе в состояние.</summary>
    void Enter();

    /// <summary>Вызывается каждый кадр, пока состояние активно.</summary>
    void Update(float dt);

    /// <summary>Вызывается один раз при выходе из состояния.</summary>
    void Exit();

}