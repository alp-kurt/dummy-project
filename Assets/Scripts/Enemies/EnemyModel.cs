using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyModel : IEnemyModel
    {
        private int m_maxHealth;
        private float m_moveSpeed;
        private int m_damage;

        private readonly ReactiveProperty<int> m_health = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> m_maxHealthRP = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> m_damageRP = new ReactiveProperty<int>(1);
        private readonly ReactiveProperty<bool> m_canMove = new ReactiveProperty<bool>(false);

        private readonly EnemyEvents m_events;

        private EnemyContext m_ctx;
        private EnemyStateMachine m_fsm;

        private const float k_OffscreenDespawnSeconds = 2.0f;
        private const float k_DeathDespawnSeconds = 1.0f;

        private int m_spawnId = 0;

        public IReadOnlyReactiveProperty<int> Health => m_health;
        public IReadOnlyReactiveProperty<int> MaxHealth => m_maxHealthRP;
        public IReadOnlyReactiveProperty<int> Damage => m_damageRP;
        public float MoveSpeed => m_moveSpeed;
        public IReadOnlyReactiveProperty<bool> CanMove => m_canMove;

        // Simple events (back-compat)
        public IObservable<int> Damaged => m_events.DamagedAmount;
        public IObservable<Unit> Died => m_events.Died;
        public IObservable<Unit> ReturnedToPool => m_events.ReturnedToPool;

        // Typed events
        public IObservable<EnemyDamagedEvent> DamagedTyped => m_events.Damaged;
        public IObservable<EnemyDiedEvent> DiedTyped => m_events.DiedTyped;
        public IObservable<EnemyReturnedToPoolEvent> ReturnedToPoolTyped => m_events.ReturnedToPoolTyped;

        internal bool IsOnScreenInternal { get; private set; }
        internal EnemyStateMachine StateMachineInternal => m_fsm;

        [Inject] public EnemyModel(EnemyEvents eventsHub) { m_events = eventsHub; }

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

            m_events.BeginLifetime(++m_spawnId);
            m_ctx.PooledReason = EnemyPooledReason.Unknown;

            m_fsm.Transition(new EnemyState_OutOfScreen());
        }

        public void Tick(float deltaTime) => m_fsm.Update(Mathf.Max(0f, deltaTime));

        public void ApplyDamage(int amount)
        {
            int dmg = Mathf.Max(0, amount);
            if (dmg <= 0) return;
            if (m_health.Value <= 0) return;

            m_health.Value = Mathf.Max(0, m_health.Value - dmg);
            m_events.RaiseDamaged(dmg, m_health.Value);

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
            {
                m_fsm.Transition(new EnemyState_Active());
            }
            else if (!isOnScreen && cur is EnemyState_Active)
            {
                m_fsm.Transition(new EnemyState_OutOfScreen());
            }
        }

        internal void SetCanMoveInternal(bool value) => m_canMove.Value = value;
        internal void EmitDiedInternal() => m_events.RaiseDied();
        internal void SwitchToPooledInternal(EnemyPooledReason reason = EnemyPooledReason.Unknown)
            => m_events.RaiseReturnedToPool(reason);
    }
}
