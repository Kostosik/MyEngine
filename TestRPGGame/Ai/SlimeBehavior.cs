using MyEngine.Ai;
using MyEngine.Ecs;
using MyEngine.Components;
using System.Numerics;

namespace TestRPGGame.Ai;

public sealed class SlimeBehavior : IBehavior
{
    private readonly StateMachine<EnemyState> _fsm;
    private readonly NavGrid _navGrid;

    // --- Данные на кадр ---
    private Entity _self = null!;
    private World _world = null!;
    private Transform _transform = null!;
    private Velocity _velocity = null!;
    private AI _ai = null!;
    private Transform? _playerTransform;
    private bool _canSeePlayer;
    private bool _inAttackRange;
    private bool _isDead;

    // --- Память ---
    private Vector2? _lastKnownPlayerPos;
    private float _memoryTimer;
    private const float MemoryDuration = 3f;

    // --- Путь ---
    private List<Vector2>? _currentPath;
    private int _pathIndex;
    private Vector2 _lastGoal;
    private float _repathTimer;
    private const float RepathInterval = 0.4f;
    private const float GoalMoveTolerance = 40f;
    private const float WaypointTolerance = 16f;

    // --- Застревание ---
    private Vector2 _lastPosition;
    private float _stuckTimer;
    private const float StuckThreshold = 0.3f;
    private const float StuckSpeedThreshold = 5f;

    // --- Separation ---
    private readonly List<Vector2> _neighborPositions = new(16);
    private const float SeparationRadius = 40f;


    public IReadOnlyList<Vector2>? CurrentPath => _currentPath;
    public Vector2? LastKnownPlayerPos => _lastKnownPlayerPos;
    public bool CanSeePlayer => _canSeePlayer;
    public SlimeBehavior(NavGrid navGrid)
    {
        _navGrid = navGrid;
        _fsm = new StateMachine<EnemyState>(EnemyState.Idle);

        _fsm.AddState(EnemyState.Idle).OnTick(IdleTick);
        _fsm.AddState(EnemyState.Chase).OnTick(ChaseTick);
        _fsm.AddState(EnemyState.Attack).OnTick(AttackTick);
        _fsm.AddState(EnemyState.Dead).OnTick(DeadTick);

        _fsm.AddAnyTransition(EnemyState.Dead, () => _isDead, priority: 100);

        // Idle → Chase: увидел ИЛИ помнит
        _fsm.AddTransition(EnemyState.Idle, EnemyState.Chase,
            () => _canSeePlayer || _memoryTimer > 0, priority: 1);

        // Chase → Attack
        _fsm.AddTransition(EnemyState.Chase, EnemyState.Attack,
            () => _inAttackRange, priority: 2);

        // Chase → Idle: не видит И память кончилась
        _fsm.AddTransition(EnemyState.Chase, EnemyState.Idle,
            () => !_canSeePlayer && _memoryTimer <= 0, priority: 1);

        // Attack → Chase: игрок убежал
        _fsm.AddTransition(EnemyState.Attack, EnemyState.Chase,
            () => !_inAttackRange, priority: 1);

        _fsm.Start();
    }

    public void Tick(Entity self, World world, float dt)
    {
        _self = self;
        _world = world;
        _transform = self.Get<Transform>()!;
        _velocity = self.Get<Velocity>()!;
        _ai = self.Get<AI>()!;

        var health = self.Get<Health>();
        _isDead = health != null && !health.IsAlive;

        var player = world.FirstWith<PlayerTag>();
        _playerTransform = player?.Get<Transform>();

        if (_playerTransform != null)
        {
            float dist = Vector2.Distance(_transform.Position, _playerTransform.Position);
            _canSeePlayer = dist < _ai.AggroRadius;
            _inAttackRange = dist < _ai.AttackRange;

            if (_canSeePlayer)
            {
                _lastKnownPlayerPos = _playerTransform.Position;
                _memoryTimer = MemoryDuration;
            }
        }
        else
        {
            _canSeePlayer = false;
            _inAttackRange = false;
        }

        if (_memoryTimer > 0) _memoryTimer -= dt;

        // Детект застревания
        float moved = Vector2.Distance(_transform.Position, _lastPosition);
        if (moved < StuckSpeedThreshold * dt) _stuckTimer += dt;
        else _stuckTimer = 0;
        _lastPosition = _transform.Position;

        _fsm.Tick(dt);

        _ai.State = _fsm.Current switch
        {
            EnemyState.Idle => AIState.Idle,
            EnemyState.Chase => AIState.Chase,
            EnemyState.Attack => AIState.Attack,
            EnemyState.Dead => AIState.Dead,
            _ => AIState.Idle
        };
    }

    private void IdleTick(float dt)
    {
        _currentPath = null;
        var desired = Steering.Arrive(
            _transform.Position, _ai.HomePosition,
            maxSpeed: _ai.MoveSpeed * 0.5f, slowRadius: 20f);
        _velocity.Value = CombineWithSeparation(desired, dt);
    }

    private void ChaseTick(float dt)
    {
        // Куда идём: игрок, если видим; иначе — последняя известная точка
        Vector2 goal;
        if (_canSeePlayer && _playerTransform != null)
            goal = _playerTransform.Position;
        else if (_lastKnownPlayerPos.HasValue)
            goal = _lastKnownPlayerPos.Value;
        else
            goal = _ai.HomePosition;

        // Репат по таймеру, по смещению цели ИЛИ при застревании
        _repathTimer -= dt;
        bool goalMoved = Vector2.Distance(goal, _lastGoal) > GoalMoveTolerance;
        bool stuck = _stuckTimer > StuckThreshold;

        if (_currentPath == null || _repathTimer <= 0 || goalMoved || stuck)
        {
            _currentPath = AStar.FindPath(_navGrid, _transform.Position, goal);
            _pathIndex = 0;
            _lastGoal = goal;
            _repathTimer = RepathInterval;

            if (stuck)
            {
                // Сброс — в следующий раз не будем думать, что застряли
                _stuckTimer = 0;
            }
        }

        // Выбор текущей точки пути
        Vector2 target;
        if (_currentPath != null && _pathIndex < _currentPath.Count)
        {
            target = _currentPath[_pathIndex];

            while (Vector2.Distance(_transform.Position, target) < WaypointTolerance
                   && _pathIndex + 1 < _currentPath.Count)
            {
                _pathIndex++;
                target = _currentPath[_pathIndex];
            }
        }
        else
        {
            target = goal; // fallback — напрямую
        }

        var desired = Steering.Seek(
            _transform.Position, target,
            maxSpeed: _ai.MoveSpeed);

        _velocity.Value = CombineWithSeparation(desired, dt);
    }

    private void AttackTick(float dt)
    {
        _currentPath = null;
        _velocity.Value = Vector2.Zero;
    }

    private void DeadTick(float dt)
    {
        _currentPath = null;
        _velocity.Value = Vector2.Zero;
    }

    private Vector2 CombineWithSeparation(Vector2 desired, float dt)
    {
        if (desired.LengthSquared() < 0.001f) return Vector2.Zero;

        _neighborPositions.Clear();
        foreach (var other in _world.With<EnemyTag>())
        {
            if (ReferenceEquals(other, _self)) continue;
            var ot = other.Get<Transform>();
            if (ot == null) continue;
            if (Vector2.Distance(_transform.Position, ot.Position) < SeparationRadius)
                _neighborPositions.Add(ot.Position);
        }

        if (_neighborPositions.Count == 0) return desired;

        var sep = Steering.Separation(
            _transform.Position, _neighborPositions,
            desiredDistance: SeparationRadius,
            maxSpeed: _ai.MoveSpeed);

        var combined = desired + sep * 0.5f;
        if (combined.Length() > _ai.MoveSpeed)
            combined = Vector2.Normalize(combined) * _ai.MoveSpeed;
        return combined;
    }
}