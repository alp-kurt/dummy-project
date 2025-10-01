using System;
using UniRx;

namespace Scripts
{
    public interface IEnemyModel : IDamager
    {
        // Movement/state (read)
        float MoveSpeed { get; }
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // Movement/state (write)
        void SetCanMove(bool value);              // <?

        // Health
        int MaxHealth { get; }
        IReadOnlyReactiveProperty<int> CurrentHealth { get; }
        IObservable<int> Damaged { get; }
        IObservable<Unit> Died { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void ResetForSpawn();

        // Damage intake
        void ReceiveDamage(int amount);
    }
}
