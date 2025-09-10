using UniRx;
using System;

namespace Scripts
{
    public interface IEnemyModel
    {
        // Core stats
        IReadOnlyReactiveProperty<int> Health { get; }
        IReadOnlyReactiveProperty<int> MaxHealth { get; }
        IReadOnlyReactiveProperty<int> Damage { get; }
        float MoveSpeed { get; }

        // Movement permission (FSM drives this)
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void ResetForSpawn();
        void Tick(float deltaTime);

        // Inputs
        void ApplyDamage(int amount);
        void SetOnScreen(bool isOnScreen);

        // Signals
        IObservable<int> Damaged { get; }
        IObservable<UniRx.Unit> Died { get; }
        IObservable<UniRx.Unit> ReturnedToPool { get; }
    }
}
