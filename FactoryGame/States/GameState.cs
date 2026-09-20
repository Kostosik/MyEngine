namespace FactoryGame.States;

public sealed class GameState
{
    public float SaveTimer;
    public long TickCount;
    public bool IsLoading;

    public void Update(float dt)
    {
        SaveTimer += dt;
        TickCount++;
    }
}