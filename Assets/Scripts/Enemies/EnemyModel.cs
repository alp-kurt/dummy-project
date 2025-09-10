using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyModel : IEnemyModel
    {
        // Stats
        private int m_maxHealth;
        private float m_moveSpeed;
        private int m_damage;

        // Reactive state
        private readonly ReactiveProperty<int> m_health = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> m_maxHealthRP = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> m_damageRP = new ReactiveProperty<int>(1);
        private readonly ReactiveProperty<bool> m_canMove = new ReactiveProperty<bool>(false);

        // FSM
        private EnemyContext m_ctx;
        private EnemyStateMachine m_fsm;

        // Timings
        private const float k_OffscreenDespawnSeconds = 2.0f;
        private const float k_DeathDespawnSeconds = 1.0f; // per your spec

        // Events
        private readonly Subject<int> m_damaged = new Subject<int>();
        private readonly Subject<Unit> m_died = new Subject<Unit>();
        private readonly Subject<Unit> m_returnedToPool = new Subject<Unit>();

        // Public props
        public IReadOnlyReactiveProperty<int> Health => m_health;
        public IReadOnlyReactiveProperty<int> MaxHealth => m_maxHealthRP;
        public IReadOnlyReactiveProperty<int> Damage => m_damageRP;
        public float MoveSpeed => m_moveSpeed;
        public IReadOnlyReactiveProperty<bool> CanMove => m_canMove;

        public IObservable<int> Damaged => m_damaged;
        public IObservable<Unit> Died => m_died;
        public IObservable<Unit> ReturnedToPool => m_returnedToPool;

        // Internal for states/presenter
        internal bool IsOnScreenInternal { get; private set; }
        internal EnemyStateMachine StateMachineInternal => m_fsm;

        public void Initialize(EnemyStats stats)
        {
            m_maxHealth = Mathf.Max(1, stats.maxHealth);
            m_moveSpeed = Mathf.Max(0f, stats.movementSpeed);
            m_damage = Mathf.Max(1, stats.damage);

            m_maxHealthRP.Value = m_maxHealth;
            m_damageRP.Value = m_damage;

            m_ctx = new EnemyContext(this, k_OffscreenDespawnSeconds, k_DeathDespawnSeconds);
            m_fsm = new EnemyStateMachine(m_ctx);

            // Default to Pooled; spawner will call ResetForSpawn → OutOfScreen
            m_health.Value = 0;
            IsOnScreenInternal = false;
            m_canMove.Value = false;

            m_fsm.Start(new EnemyState_Pooled());
        }

        public void ResetForSpawn()
        {
            m_health.Value = m_maxHealth;
            IsOnScreenInternal = false;
            m_canMove.Value = true; // Off-screen still moves toward player
            m_fsm.Transition(new EnemyState_OutOfScreen());
        }

        public void Tick(float deltaTime)
        {
            m_fsm.Update(Mathf.Max(0f, deltaTime));
        }

        public void ApplyDamage(int amount)
        {
            int dmg = Mathf.Max(0, amount);
            if (dmg <= 0) return;
            if (m_health.Value <= 0) return;

            m_health.Value = Mathf.Max(0, m_health.Value - dmg);
            m_damaged.OnNext(dmg);

            if (m_health.Value <= 0)
            {
                m_fsm.Transition(new EnemyState_Dead());
            }
        }

        public void SetOnScreen(bool isOnScreen)
        {
            IsOnScreenInternal = isOnScreen;

            // Visibility-driven transitions:
            var cur = m_fsm.Current;
            if (isOnScreen && cur is EnemyState_OutOfScreen)
            {
                m_fsm.Transition(new EnemyState_Active());
            }
            else if (!isOnScreen && cur is EnemyState_Active)
            {
                m_fsm.Transition(new EnemyState_OutOfScreen());
            }
        }

        // ---- called by states ----
        internal void SetCanMoveInternal(bool value) => m_canMove.Value = value;
        internal void EmitDiedInternal() => m_died.OnNext(Unit.Default);
        internal void SwitchToPooledInternal() => m_returnedToPool.OnNext(Unit.Default);
    }
}
