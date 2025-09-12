using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyModel : IEnemyModel
    {
        // Stats (non-health)
        private float m_moveSpeed;
        private readonly ReactiveProperty<bool> m_canMove = new(false);

        // Events
        private readonly Subject<Unit> m_returned = new();

        // FSM plumbing
        private EnemyContext m_ctx;
        private EnemyStateMachine m_fsm;

        // Linked health
        private IEnemyHealthModel m_health;
        private bool m_deadTransitioned;

        private const float k_OffscreenDespawnSeconds = 2.0f;
        private const float k_DeathDespawnSeconds = 1.0f;

        // Public props
        public float MoveSpeed => m_moveSpeed;
        public IReadOnlyReactiveProperty<bool> CanMove => m_canMove;
        public IObservable<Unit> ReturnedToPool => m_returned;

        // Internal for context/states
        internal bool IsOnScreenInternal { get; private set; }
        internal EnemyStateMachine StateMachineInternal => m_fsm;
        internal void SetCanMoveInternal(bool v) => m_canMove.Value = v;

        private IDisposable m_healthDiedSub;

        // Called by Dead state
        internal void EmitDiedInternal()
        {
            if (m_deadTransitioned) return;
            m_deadTransitioned = true;
        }

        internal void SwitchToPooledInternal() => m_returned.OnNext(Unit.Default);

        public void Initialize(EnemyStats stats)
        {
            m_moveSpeed = Mathf.Max(0f, stats.movementSpeed);

            m_ctx = new EnemyContext(this, k_OffscreenDespawnSeconds, k_DeathDespawnSeconds);
            m_fsm = new EnemyStateMachine(m_ctx);

            IsOnScreenInternal = false;
            m_canMove.Value = false;
            m_deadTransitioned = false;

            m_fsm.Start(new EnemyState_Pooled());
        }

        public void SetHealth(IEnemyHealthModel health)
        {
            m_health = health;

            m_healthDiedSub?.Dispose();
            m_healthDiedSub = m_health.Died.Subscribe(_ =>
            {
                if (!m_deadTransitioned)
                {
                    m_deadTransitioned = true;
                    m_fsm.Transition(new EnemyState_Dead());
                }
            }
            );
        }

        public void ResetForSpawn()
        {
            m_deadTransitioned = false;
            IsOnScreenInternal = false;
            m_canMove.Value = true;

            // Ensure health resets for new lifetime
            m_health?.ResetFull();

            m_fsm.Transition(new EnemyState_OutOfScreen());
        }

        public void Tick(float deltaTime) => m_fsm.Update(Mathf.Max(0f, deltaTime));

        public void SetOnScreen(bool isOnScreen)
        {
            IsOnScreenInternal = isOnScreen;

            var cur = m_fsm.Current;
            if (isOnScreen && cur is EnemyState_OutOfScreen)
                m_fsm.Transition(new EnemyState_Active());
            else if (!isOnScreen && cur is EnemyState_Active)
                m_fsm.Transition(new EnemyState_OutOfScreen());
        }
    }
}
