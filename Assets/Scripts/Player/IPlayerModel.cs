using System;
using UniRx;

namespace Scripts
{
    public interface IPlayerModel
    {
        // Normalized (0..1) health for easy UI binding
        IReadOnlyReactiveProperty<float> CurrentHealth { get; }

        // Expose max health (hp units) for mapping integer enemy damage
        float MaxHealth { get; }

        // Fired once when health reaches zero
        IObservable<Unit> Died { get; }

        // Apply damage in "hp" units (will be normalized internally)
        void ApplyDamage(int amount);
    }
}
