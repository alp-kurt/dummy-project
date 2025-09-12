using System;
using UniRx;

namespace Scripts
{
    public interface IEnemyHealthModel : IDamageable
    {
        // Normalized [0..1] for UI binding
        IReadOnlyReactiveProperty<float> CurrentHealth01 { get; }
        int MaxHealth { get; }

        // Signals
        IObservable<Unit> Died { get; }
        IObservable<int> Damaged { get; }

        // Commands
        void Initialize(int maxHealth);

        void ResetFull();
    }
}