using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyModel : IEnemyModel
    {
        // Stats
        private float _moveSpeed;
        private readonly ReactiveProperty<bool> _canMove = new(false);
        private int _damage;

        // Events
        private readonly Subject<Unit> _returned = new();

        // FSM plumbing
        private EnemyContext _ctx;
        private EnemyStateMachine _fsm;

        // Linked health
        private IEnemyHealthModel _health;
        private bool _deadTransitioned;

        private const float k_OffscreenDespawnSeconds = 0.5f;
        private const float k_DeathDespawnSeconds = 1.0f;

        // Public props
        public float MoveSpeed => _moveSpeed;
        public int Damage => _damage;
        public IReadOnlyReactiveProperty<bool> CanMove => _canMove;
        public IObservable<Unit> ReturnedToPool => _returned;

        // Internal for context/states
        internal bool IsOnScreenInternal { get; private set; }
        internal EnemyStateMachine StateMachineInternal => _fsm;
        internal void SetCanMoveInternal(bool v) => _canMove.Value = v;

        private IDisposable _healthDiedSub;

        // Called by Dead state
        internal void EmitDiedInternal()
        {
            if (_deadTransitioned) return;
            _deadTransitioned = true;
        }

        internal void SwitchToPooledInternal() => _returned.OnNext(Unit.Default);

        public void Initialize(EnemyStats stats)
        {
            _moveSpeed = Mathf.Max(0f, stats.MovementSpeed);
            _damage = Mathf.Max(0, stats.Damage);

            // UseUnscaledTime defaults to false; flip via the EnemyContext ctor overload if desired.
            _ctx = new EnemyContext(this, k_OffscreenDespawnSeconds, k_DeathDespawnSeconds);
            _fsm = new EnemyStateMachine(_ctx);

            IsOnScreenInternal = false;
            _canMove.Value = false;
            _deadTransitioned = false;

            // Allocation-free start
            _fsm.Start(EnemyState_Pooled.Instance);
        }

        public void SetHealth(IEnemyHealthModel health)
        {
            _health = health;

            _healthDiedSub?.Dispose();
            _healthDiedSub = _health.Died.Subscribe(_ =>
            {
                if (!_deadTransitioned)
                {
                    _deadTransitioned = true;
                    _fsm.Transition(EnemyState_Dead.Instance);
                }
            });
        }

        public void ResetForSpawn()
        {
            _deadTransitioned = false;
            IsOnScreenInternal = false;
            _canMove.Value = true;

            // Safety: cancel any timers from a previous lifetime
            _ctx?.CancelAllTimers();

            _health?.ResetFull();

            _fsm.Transition(EnemyState_OutOfScreen.Instance);
        }

        public void Tick(float deltaTime) => _fsm.Update(Mathf.Max(0f, deltaTime));

        public void SetOnScreen(bool isOnScreen)
        {
            IsOnScreenInternal = isOnScreen;

            var cur = _fsm.Current;
            if (isOnScreen && cur is EnemyState_OutOfScreen)
                _fsm.Transition(EnemyState_Active.Instance);   
            else if (!isOnScreen && cur is EnemyState_Active)
                _fsm.Transition(EnemyState_OutOfScreen.Instance); 
        }
    }
}
