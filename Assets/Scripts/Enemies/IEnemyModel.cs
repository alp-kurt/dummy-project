using UniRx;
using System;

namespace Scripts
{
    public interface IEnemyModel
    {
        // Data
        IReadOnlyReactiveProperty<int> Health { get; }
        IReadOnlyReactiveProperty<int> MaxHealth { get; }
        IReadOnlyReactiveProperty<int> Damage { get; }
        float MoveSpeed { get; }
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void ResetForSpawn();
        void Tick(float deltaTime);

        // Inputs from world
        void ApplyDamage(int amount);
        void SetOnScreen(bool isOnScreen);

        // Simple events (no payloads)
        IObservable<Unit> Damaged { get; }
        IObservable<Unit> Died { get; }
        IObservable<Unit> ReturnedToPool { get; }
    }
}
