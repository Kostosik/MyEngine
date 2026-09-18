namespace FactoryGame;

public sealed class GameState
{
    public float SaveTimer;
    public long TickCount;

    public void Update(float dt)
    {
        SaveTimer += dt;
        TickCount++;
    }
}