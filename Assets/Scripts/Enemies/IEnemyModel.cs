using UniRx;
using System;

namespace Scripts
{
    public interface IEnemyModel : IDamager
    {
        // Movement/state data
        float MoveSpeed { get; }
        IReadOnlyReactiveProperty<bool> CanMove { get; }

        // Lifecycle
        void Initialize(EnemyStats stats);
        void SetHealth(IEnemyHealthModel health); 
        void ResetForSpawn();
        void Tick(float deltaTime);

        // Inputs from world
        void SetOnScreen(bool isOnScreen);

        // Pooling
        IObservable<Unit> ReturnedToPool { get; }
    }
}