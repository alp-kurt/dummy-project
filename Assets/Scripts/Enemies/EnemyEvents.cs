using System;
using UniRx;

namespace Scripts
{
    public sealed class EnemyEvents
    {
        private readonly Subject<EnemyDamagedEvent> m_damaged = new Subject<EnemyDamagedEvent>();
        private readonly Subject<EnemyDiedEvent> m_died = new Subject<EnemyDiedEvent>();
        private readonly Subject<EnemyReturnedToPoolEvent> m_returned = new Subject<EnemyReturnedToPoolEvent>();

        // Preferred typed streams
        public IObservable<EnemyDamagedEvent> Damaged => m_damaged;
        public IObservable<EnemyDiedEvent> DiedTyped => m_died;
        public IObservable<EnemyReturnedToPoolEvent> ReturnedToPoolTyped => m_returned;

        // Legacy simple streams (keep pool/presenter compatibility)
        public IObservable<int> DamagedAmount => m_damaged.Select(e => e.Amount);
        public IObservable<Unit> Died => m_died.Select(_ => Unit.Default);
        public IObservable<Unit> ReturnedToPool => m_returned.Select(_ => Unit.Default);

        public int CurrentSpawnId { get; private set; }
        public void BeginLifetime(int spawnId) => CurrentSpawnId = spawnId;

        public void RaiseDamaged(int amount, int newHealth)
            => m_damaged.OnNext(new EnemyDamagedEvent(amount, newHealth));

        public void RaiseDied()
            => m_died.OnNext(new EnemyDiedEvent(CurrentSpawnId));

        public void RaiseReturnedToPool(EnemyPooledReason reason)
            => m_returned.OnNext(new EnemyReturnedToPoolEvent(CurrentSpawnId, reason));

        // Call only on actual destroy, not during regular pool cycles
        public void DisposeForDestroy()
        {
            m_damaged.OnCompleted(); m_died.OnCompleted(); m_returned.OnCompleted();
            m_damaged.Dispose(); m_died.Dispose(); m_returned.Dispose();
        }
    }
}
