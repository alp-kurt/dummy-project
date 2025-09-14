using System;
using UniRx;

namespace Scripts
{
    public interface IPlayerHealthModel : IDamageable, IHealable
    {
        // Normalized (0..1) health for UI binding
        IReadOnlyReactiveProperty<float> CurrentHealth01 { get; }

        // Max health in HP units (for gameplay)
        float MaxHealth { get; }

        // Fired once when health reaches zero
        IObservable<Unit> Died { get; }

        // Convenience streams for effects
        IObservable<int> Damaged { get; }
        IObservable<int> Healed { get; }

    }
}
