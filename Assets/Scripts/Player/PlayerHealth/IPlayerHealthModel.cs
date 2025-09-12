using System;
using UniRx;

namespace Scripts
{
    public interface IPlayerHealthModel : IDamageable
    {
        // Normalized (0..1) health for UI binding
        IReadOnlyReactiveProperty<float> CurrentHealth01 { get; }

        // Max health in HP units (for gameplay)
        float MaxHealth { get; }

        // Fired once when health reaches zero; stays silent until ResetFull()
        IObservable<Unit> Died { get; }

        // Optional convenience streams for effects
        IObservable<int> Damaged { get; }
        IObservable<int> Healed { get; }


        // Commands
        void Heal(int amountHp);
        void ResetFull();

    }
}
