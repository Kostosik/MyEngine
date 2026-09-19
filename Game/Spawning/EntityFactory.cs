using MyEngine.Ai;
using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Game.Ai;
using MyEngine.Game.Components;
using MyEngine.Physics;
using System.Numerics;

namespace TestRPGGame.Spawning;

public static class EntityFactory
{
    public static Entity CreatePlayer(World world, Vector2 position)
    {
        Assert.NotNull(world, "World is null in CreatePlayer");
        var e = world.Create();
        e.Add(new PlayerTag());
        e.Add(new Transform { Position = position });
        e.Add(new Velocity());
        e.Add(new TopDownController { MaxSpeed = 220f });
        e.Add(new Interactor());
        e.Add(new Collider
        {
            Size = new Vector2(32, 32),
            IsStatic = false,
            Layer = Layer.Player,
            CollidesWith = Layer.Wall | Layer.Enemy
        });
        var sprite = new Sprite
        {
            Size = new Vector2(32, 32),
            Color = new Vector4(1f, 0.6f, 0.2f, 1f)
        };
        e.Add(sprite);
        e.Add(new Health { Hp = 100, MaxHp = 100, BaseColor=sprite.Color });
        e.Add(new Attack
        {
            Damage = 15,
            Range = 60f,
            Cooldown = 0.35f
        });
        

        return e;
    }

    public static Entity CreateWall(World world, Vector2 center, Vector2 size)
    {
        var e = world.Create();
        e.Add(new WallTag());
        e.Add(new Transform { Position = center });
        e.Add(new Collider
        {
            Size = size,
            IsStatic = true,
            Layer = Layer.Wall,
            CollidesWith = Layer.Player | Layer.Enemy
        });
        e.Add(new Sprite
        {
            Size = size,
            Color = new Vector4(0.28f, 0.30f, 0.34f, 1f)
        });
        return e;
    }

    public static Entity CreateEnemy(
    World world,
    Vector2 position,
    NavGrid navGrid,
    int hp = 30,
    int damage = 10,
    float moveSpeed = 90f,
    float aggroRadius = 220f,
    float attackRange = 40f,
    int xpReward = 10)
    {
        var e = world.Create();
        e.Add(new EnemyTag());
        e.Add(new Transform { Position = position });

        bool isBoss = hp >= 100;
        float size = isBoss ? 44f : 28f;
        var color = isBoss
            ? new Vector4(0.85f, 0.32f, 0.32f, 1f)  // красный элитный
            : new Vector4(0.55f, 0.85f, 0.35f, 1f); // зелёный слизень

        e.Add(new Velocity());
        e.Add(new Collider
        {
            Size = new Vector2(size, size),
            IsStatic = false,
            Layer = Layer.Enemy,
            CollidesWith = Layer.Wall | Layer.Player
        });
        var sprite = new Sprite {
            Size = new Vector2(size, size),
            Color = color
        };
        e.Add(sprite);
        e.Add(new Health { Hp = hp, MaxHp = hp ,BaseColor = sprite.Color });
        e.Add(new Attack
        {
            Damage = damage,
            Range = attackRange + (isBoss ? 12f : 0f),
            Cooldown = isBoss ? 1.4f : 1.2f
        });
        e.Add(new AI
        {
            MoveSpeed = moveSpeed,
            AggroRadius = aggroRadius,
            AttackRange = attackRange + (isBoss ? 12f : 0f),
            HomePosition = position
        });
        var behavior = new SlimeBehavior(navGrid);
        e.Add(new BehaviorComponent(behavior));
        e.Add(new Experience { Reward = xpReward });
        return e;
    }

    public static Entity CreateNpc(World world, string id, string name, Vector2 position, string[] lines)
    {
        var e = world.Create();
        e.Add(new NpcTag());
        e.Add(new Transform { Position = position });
        e.Add(new Interactable { Id = id, Radius = 70f, Prompt = $"Talk to {name}", Speaker = name });
        e.Add(new DialogueData { Lines = lines });
        e.Add(new Sprite
        {
            Size = new Vector2(32, 32),
            Color = new Vector4(0.55f, 0.65f, 1f, 1f)
        });
        return e;
    }

    public static Entity CreatePickup(World world, Vector2 position, string kind, string id)
    {
        var e = world.Create();
        e.Add(new PickupTag());
        var color = kind == "spark"
            ? new Vector4(1f, 0.95f, 0.4f, 1f) // жёлтая искра
            : new Vector4(0.7f, 1f, 0.7f, 1f);
        e.Add(new Transform { Position = position });
        e.Add(new Sprite
        {
            Size = new Vector2(20, 20),
            Color = color
        });
        e.Add(new Pickup
        {
            Id = id,
            Kind = kind
        });
        return e;
    }
    public static Entity CreateLighthouse(World world, Vector2 position)
    {
        var e = world.Create();
        e.Add(new Transform { Position = position });

        // Сама башня — большой серый блок
        e.Add(new Sprite
        {
            Size = new Vector2(80, 80),
            Color = new Vector4(0.4f, 0.45f, 0.55f, 1f)
        });

        // Интерактив — по E
        e.Add(new Interactable
        {
            Id = "lighthouse",
            Radius = 100f,
            Prompt = "Inspect the Lighthouse",
            Speaker = "Lighthouse"
        });

        return e;
    }
}