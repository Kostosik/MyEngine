using MyEngine.Diagnostics;
using MyEngine.Ecs;
using MyEngine.Components;

namespace TestRPGGame.Systems;

public sealed class ProgressionSystem : ISystem
{
    private readonly Entity _player;
    private readonly GameState _state;
    private int _lastKnownLevel;

    private const int HpPerLevel = 15;
    private const int DamagePerLevel = 1;

    public ProgressionSystem(Entity player, GameState state)
    {
        _player = player;
        _state = state;
        _lastKnownLevel = state.PlayerLevel;
    }

    public void Update(World world, float dt)
    {
        while (_lastKnownLevel < _state.PlayerLevel)
        {
            _lastKnownLevel++;
            ApplyLevelBonus(_lastKnownLevel);
        }
    }

    private void ApplyLevelBonus(int newLevel)
    {
        var hp = _player.Get<Health>();
        var attack = _player.Get<Attack>();

        if (hp != null)
        {
            hp.MaxHp += HpPerLevel;
            hp.Hp += HpPerLevel; // лечим на прирост
            if (hp.Hp > hp.MaxHp) hp.Hp = hp.MaxHp;
        }

        if (attack != null)
            attack.Damage += DamagePerLevel;

        Log.Info("Info",$"LEVEL UP! Now level {newLevel}. MaxHP={_player.Get<Health>()?.MaxHp}, Damage={_player.Get<Attack>()?.Damage}");
    }
}