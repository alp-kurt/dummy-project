using System;
using UniRx;

namespace Scripts
{
    /// <summary>
    /// Lifetime owner for a created enemy. Dispose-like Release() returns it to the pool.
    /// </summary>
    public interface IEnemyHandle
    {
        EnemyView View { get; }
        EnemyPresenter Presenter { get; }

        /// <summary>
        /// Shows the enemy (activates view, resets state as "spawned").
        /// </summary>
        void Spawn();

        /// <summary>
        /// Hides the enemy (stops movement, deactivates view).
        /// </summary>
        void Despawn();

        /// <summary>
        /// Tears down presenter subscriptions and returns the view to the pool.
        /// Calls Despawn() internally to ensure proper cleanup.
        /// </summary>
        void Release();

        /// <summary>
        /// Pooling signal (sourced from the model, exposed via the presenter) for spawners.
        /// </summary>
        IObservable<Unit> ReturnedToPool { get; }
    }
}
