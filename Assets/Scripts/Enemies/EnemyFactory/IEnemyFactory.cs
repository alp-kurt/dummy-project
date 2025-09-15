using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Creates fully-wired Enemy units (Model + View + Presenter) using the pooled EnemyView.
    /// </summary>
    public interface IEnemyFactory
    {
        /// <summary>
        /// Creates an enemy using the given stats and initial world position.
        /// The returned handle exposes Spawn/Despawn helpers and Release() to return to pool.
        /// </summary>
        IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition);
    }
}
