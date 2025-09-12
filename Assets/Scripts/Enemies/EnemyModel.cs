using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyModel : IEnemyModel, IDamageable
    {
        // Stats
        private int m_maxHealth;
        private float m_moveSpeed;
        private int m_damage;

        // Reactive data
        private readonly ReactiveProperty<int> m_health = new(0);
        private readonly ReactiveProperty<int> m_maxHealthRP = new(0);
        private readonly ReactiveProperty<int> m_damageRP = new(1);
        private readonly ReactiveProperty<bool> m_canMove = new(false);

        // Events (Unit)
        private readonly Subject<Unit> m_damaged = new();
        private readonly Subject<Unit> m_died = new();
        private readonly Subject<Unit> m_returned = new();

        // FSM plumbing
        private EnemyContext m_ctx;
        private EnemyStateMachine m_fsm;

        private const float k_OffscreenDespawnSeconds = 2.0f;
        private const float k_DeathDespawnSeconds = 1.0f;

        // Public props
        public IReadOnlyReactiveProperty<int> Health => m_health;
        public IReadOnlyReactiveProperty<int> MaxHealth => m_maxHealthRP;
        public IReadOnlyReactiveProperty<int> Damage => m_damageRP;
        public float MoveSpeed => m_moveSpeed;
        public IReadOnlyReactiveProperty<bool> CanMove => m_canMove;

        public IObservable<Unit> Damaged => m_damaged;
        public IObservable<Unit> Died => m_died;
        public IObservable<Unit> ReturnedToPool => m_returned;

        // Internal for context/states
        internal bool IsOnScreenInternal { get; private set; }
        internal EnemyStateMachine StateMachineInternal => m_fsm;
        internal void SetCanMoveInternal(bool v) => m_canMove.Value = v;
        internal void EmitDiedInternal() => m_died.OnNext(Unit.Default);
        internal void SwitchToPooledInternal() => m_returned.OnNext(Unit.Default);

        public void Initialize(EnemyStats stats)
        {
            m_maxHealth = Mathf.Max(1, stats.maxHealth);
            m_moveSpeed = Mathf.Max(0f, stats.movementSpeed);
            m_damage = Mathf.Max(1, stats.damage);

            m_maxHealthRP.Value = m_maxHealth;
            m_damageRP.Value = m_damage;

            m_ctx = new EnemyContext(this, k_OffscreenDespawnSeconds, k_DeathDespawnSeconds);
            m_fsm = new EnemyStateMachine(m_ctx);

            m_health.Value = 0;
            IsOnScreenInternal = false;
            m_canMove.Value = false;

            m_fsm.Start(new EnemyState_Pooled());
        }

        public void ResetForSpawn()
        {
            m_health.Value = m_maxHealth;
            IsOnScreenInternal = false;
            m_canMove.Value = true;

            m_fsm.Transition(new EnemyState_OutOfScreen());
        }

        public void Tick(float deltaTime) => m_fsm.Update(Mathf.Max(0f, deltaTime));

        public void ApplyDamage(int amount)
        {
            int dmg = Mathf.Max(0, amount);
            if (dmg <= 0) return;
            if (m_health.Value <= 0) return;

            m_health.Value = Mathf.Max(0, m_health.Value - dmg);
            m_damaged.OnNext(Unit.Default);

            if (m_health.Value <= 0)
            {
                m_fsm.Transition(new EnemyState_Dead());
            }
        }

        public void SetOnScreen(bool isOnScreen)
        {
            IsOnScreenInternal = isOnScreen;

            var cur = m_fsm.Current;
            if (isOnScreen && cur is EnemyState_OutOfScreen)
                m_fsm.Transition(new EnemyState_Active());
            else if (!isOnScreen && cur is EnemyState_Active)
                m_fsm.Transition(new EnemyState_OutOfScreen());
        }

        void IDamageable.ReceiveDamage(int amount) => ApplyDamage(amount);
    }
}
