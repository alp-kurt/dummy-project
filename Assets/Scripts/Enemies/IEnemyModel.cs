using UniRx;
using System;

namespace Scripts
{
    public interface IEnemyModel
    {
        IReadOnlyReactiveProperty<int> Health { get; }
        IReadOnlyReactiveProperty<int> MaxHealth { get; }
        IReadOnlyReactiveProperty<int> Damage { get; }
        float MoveSpeed { get; }
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void ResetForSpawn();
        void Tick(float deltaTime);

        // Inputs
        void ApplyDamage(int amount);
        void SetOnScreen(bool isOnScreen);

        // Legacy simple events
        IObservable<int> Damaged { get; }
        IObservable<Unit> Died { get; }
        IObservable<Unit> ReturnedToPool { get; }

        // Typed events (SpawnId / Reason)
        IObservable<EnemyDamagedEvent> DamagedTyped { get; }
        IObservable<EnemyDiedEvent> DiedTyped { get; }
        IObservable<EnemyReturnedToPoolEvent> ReturnedToPoolTyped { get; }
    }
}
