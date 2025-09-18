using System;
using UniRx;

namespace Scripts
{
    public interface IEnemyModel : IDamager
    {
        // Movement/state (read)
        float MoveSpeed { get; }
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // NEW: movement/state (write)
        void SetCanMove(bool value);              // <?

        // Health
        int MaxHealth { get; }
        IReadOnlyReactiveProperty<int> CurrentHealth { get; }
        IObservable<int> Damaged { get; }
        IObservable<Unit> Died { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void ResetForSpawn();

        // (Optional legacy; can keep as no-op)
        void Tick(float deltaTime);

        // World input
        void SetOnScreen(bool isOnScreen);

        // Pooling
        IObservable<Unit> ReturnedToPool { get; }
        void NotifyReturnedToPool();              // <?

        // Damage intake
        void ReceiveDamage(int amount);
    }
}
