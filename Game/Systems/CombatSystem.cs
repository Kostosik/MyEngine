using MyEngine;
using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Events;
using MyEngine.Game.Components;
using MyEngine.Game.Events;
using MyEngine.InputEngine;
using MyEngine.Time;
using System.Numerics;

namespace TestRPGGame.Systems;

public sealed class CombatSystem : ISystem
{
    private readonly Application _game;
    private readonly Entity _player;
    private readonly GameState _state;
    private readonly DamagePopupSystem _popups;
    private readonly EventBus _events;

    public SystemPhase Phase => SystemPhase.PostUpdate;
    public int Priority => 10;

    public CombatSystem(Application game, Entity player, GameState state, DamagePopupSystem popups, EventBus events)
    {
        _game = game;
        _player = player;
        _state = state;
        _popups = popups;
        _events = events;
    }

    public void Update(World world, float dt)
    {
        TickTimers(world, dt);
        HandlePlayerAttack(world);
        HandleEnemyAttacks(world);
        HandleDeaths(world);
    }

    private void TickTimers(World world, float dt)
    {
        foreach (var e in world.Entities)
        {
            if (!e.IsAlive) continue;

            var attack = e.Get<Attack>();
            if (attack != null)
            {
                attack.TimeSinceLast += dt;
                if (attack.ActiveTimer > 0) attack.ActiveTimer -= dt;
            }

            var health = e.Get<Health>();
            if (health != null)
            {
                if (health.InvulnTimer > 0) health.InvulnTimer -= dt;
                if (health.HitFlashTimer > 0) health.HitFlashTimer -= dt;
            }
        }
    }

    private void HandlePlayerAttack(World world)
    {
        var attack = _player.Get<Attack>();
        var playerT = _player.Get<Transform>();
        if (attack == null || playerT == null) return;

        if (!_game.Input.WasActionPressed(GameAction.Attack)) return;
        if (!attack.IsReady) return;  // кулдаун
        attack.TimeSinceLast = 0;     // после первого удара кулдаун 0.35с → второй не пройдёт
        attack.ActiveTimer = 0.15f;

        foreach (var e in world.With<EnemyTag>())
        {
            var et = e.Get<Transform>();
            var hp = e.Get<Health>();
            if (et == null || hp == null || !hp.IsAlive) continue;

            float dist = Vector2.Distance(playerT.Position, et.Position);
            if (dist > attack.Range) continue;

            ApplyDamage(hp, attack.Damage);
            hp.HitFlashTimer = 0.15f;
            _popups.Spawn(et.Position, attack.Damage);
            _events.Publish(new EnemyDamagedEvent { Position = et.Position });

            TimeEngine.HitStop(0.05f);
        }
    }

    private void HandleEnemyAttacks(World world)
    {
        var playerHp = _player.Get<Health>();
        var playerT = _player.Get<Transform>();
        if (playerHp == null || playerT == null) return;



        foreach (var e in world.With<EnemyTag>())
        {
            var ai = e.Get<AI>();
            var attack = e.Get<Attack>();
            var et = e.Get<Transform>();
            var hp = e.Get<Health>();
            if (ai == null || attack == null || et == null || hp == null) continue;
            if (!hp.IsAlive) continue;
            if (ai.State != AIState.Attack) continue;

            float dist = Vector2.Distance(et.Position, playerT.Position);
            if (dist > attack.Range + 8f) continue;
            if (!attack.IsReady) continue;

            attack.TimeSinceLast = 0;
            attack.ActiveTimer = 0.15f;

            if (playerHp.InvulnTimer > 0) continue;

            ApplyDamage(playerHp, attack.Damage);
            _events.Publish(new PlayerDamagedEvent
            {
                Amount = attack.Damage,
                Position = playerT.Position
            });
            playerHp.InvulnTimer = 0.6f;
            playerHp.HitFlashTimer = 0.15f;
            _popups.Spawn(playerT.Position, attack.Damage,
                new Vector4(1f, 0.35f, 0.35f, 1f)); // красный для урона игроку
        }
    }

    private static void ApplyDamage(Health hp, int amount)
    {
        Assert.NotNull(hp);
        Assert.True(amount >= 0, $"Negative damage: {amount}");
        hp.Hp -= amount;
        if (hp.Hp < 0) hp.Hp = 0;
    }

    private void HandleDeaths(World world)
    {
        foreach (var e in world.With<EnemyTag>())
        {
            var hp = e.Get<Health>();
            if (hp == null || hp.IsAlive) continue;

            var xp = e.Get<Experience>();
            var t = e.Get<Transform>();
            _events.Publish(new EnemyKilledEvent
            {
                Enemy = e,
                XpReward = xp?.Reward ?? 0,
                Position = t?.Position ?? Vector2.Zero
            });

            _state.EnemiesKilled++;
            world.Destroy(e);
        }
        var playerHp = _player.Get<Health>();
        if (playerHp != null && !playerHp.IsAlive)
        {
            var vel = _player.Get<Velocity>();
            if (vel != null) vel.Value = Vector2.Zero;
        }
    }
}