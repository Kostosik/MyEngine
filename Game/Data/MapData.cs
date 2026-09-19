namespace TestRPGGame.Data;

public sealed class MapData
{
    public List<WallData> Walls { get; set; } = new();
    public List<EnemyData> Enemies { get; set; } = new();
    public List<NpcData> Npcs { get; set; } = new();
    public List<PickupData> Pickups { get; set; } = new();
    public LighthouseData? Lighthouse { get; set; } // NEW
}

public sealed class WallData
{
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
}

public sealed class EnemyData
{
    public string Type { get; set; } = "slime";
    public float X { get; set; }
    public float Y { get; set; }
    public int Hp { get; set; } = 30;
    public int Damage { get; set; } = 10;
    public float Speed { get; set; } = 90f;
    public float AggroRadius { get; set; } = 220f;
    public int Xp { get; set; } = 10;
}

public sealed class NpcData
{
    public string Id { get; set; } = "npc";
    public string Name { get; set; } = "Keeper";
    public float X { get; set; }
    public float Y { get; set; }
    public List<string> Lines { get; set; } = new();
}

public sealed class PickupData
{
    public string Id { get; set; } = "pickup";
    public string Kind { get; set; } = "spark";
    public float X { get; set; }
    public float Y { get; set; }
}

public sealed class LighthouseData
{
    public float X { get; set; }
    public float Y { get; set; }
}