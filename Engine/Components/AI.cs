using MyEngine.Ecs;
using System.Numerics;

namespace MyEngine.Components;

public enum AIState { Idle, Chase, Attack, Dead }

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(Velocity))]
public sealed class AI
{
    public AIState State = AIState.Idle;
    public float MoveSpeed;
    public float AggroRadius;
    public float AttackRange;
    public Vector2 HomePosition;
}