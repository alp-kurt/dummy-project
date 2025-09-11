using System;
using UniRx;

namespace Scripts
{
    public interface IPlayerHealthModel
    {
        // Normalized (0..1) health for UI binding
        IReadOnlyReactiveProperty<float> CurrentHealth01 { get; }

        // Max health in HP units (for gameplay)
        float MaxHealth { get; }

        // Fired once when health reaches zero
        IObservable<Unit> Died { get; }

        // Commands
        void ApplyDamage(int amountHp);
        void Heal(int amountHp);
        void ResetFull();
    }
}
