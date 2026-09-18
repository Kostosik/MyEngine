using MyEngine.Game.Components;
using MyEngine.Components;
using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Game.Systems;

public sealed class QuestSystem
{
    private readonly GameState _state;
    private readonly Entity _player;
    private readonly DamagePopupSystem _popups;

    public QuestSystem(GameState state, Entity player, DamagePopupSystem popups)
    {
        _state = state;
        _player = player;
        _popups = popups;
    }

    public string[] GetDialogueLines(Interactable npc, Entity npcEntity, Vector2 npcPosition)
    {
        var dialogue = npcEntity.Get<DialogueData>();
        var defaultLines = dialogue?.Lines ?? System.Array.Empty<string>();

        if (npc.Id != "keeper")
            return defaultLines;

        // Стадия 1 — ещё не все искры найдены
        if (_state.SparksCollected < _state.SparksTotal)
        {
            return new[]
            {
                "The lighthouse is still dark.",
                $"You have found {_state.SparksCollected} of {_state.SparksTotal} sparks.",
                "Search the island. I will wait here."
            };
        }

        // Стадия 2 — все искры на руках, награда ещё не выдана
        if (!_state.QuestRewardGiven)
        {
            _state.QuestRewardGiven = true;
            GrantReward(npcPosition);
            return new[]
            {
                "You found them all!",
                "Take this blessing — the light of the old keepers.",
                "(+50 max HP, +5 sword damage)",
                "The lighthouse... it is ready. Go."
            };
        }

        // Стадия 3 — награда выдана
        return new[]
        {
            "The light awaits you at the lighthouse."
        };
    }

    private void GrantReward(Vector2 npcPosition)
    {
        var hp = _player.Get<Health>();
        var attack = _player.Get<Attack>();

        if (hp != null)
        {
            hp.MaxHp += 50;
            hp.Hp = hp.MaxHp;
        }
        if (attack != null)
        {
            attack.Damage += 5;
        }

        _popups.SpawnText(npcPosition + new Vector2(0, -20),
            "Quest complete!", new Vector4(0.4f, 1f, 0.5f, 1f));
    }
}