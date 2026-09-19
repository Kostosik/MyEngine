namespace TestRPGGame;

public enum GamePhase { Playing, Dead, Victory }

public sealed class GameState
{
    public GamePhase Phase = GamePhase.Playing;

    public int PlayerLevel = 1;
    public int PlayerXp;
    public int XpToNext => 80 + PlayerLevel * 40;
    public int SkillPoints;

    public float PlayerMoveSpeedMultiplier = 1f;
    public int EnemiesTotal;
    public int EnemiesKilled;

    public int SparksTotal;
    public int SparksCollected;
    public bool QuestRewardGiven;

    public int LastXpGained;
    public float LastXpGainedTimer;

    public bool IsLoading;

    public void AddXp(int amount)
    {
        PlayerXp += amount;
        LastXpGained = amount;
        LastXpGainedTimer = 2f;

        while (PlayerXp >= XpToNext)
        {
            PlayerXp -= XpToNext;
            PlayerLevel++;
            SkillPoints++;
        }
    }

    public void Tick(float dt)
    {
        if (LastXpGainedTimer > 0)
            LastXpGainedTimer -= dt;
    }

    public string QuestText()
    {
        if (SparksCollected < SparksTotal)
            return $"Find the sparks ({SparksCollected} / {SparksTotal})";
        if (!QuestRewardGiven)
            return "Return to the Keeper";
        return "Light the lighthouse";
    }

    // NEW
    public bool CanLightLighthouse()
        => SparksCollected >= SparksTotal && QuestRewardGiven;
}