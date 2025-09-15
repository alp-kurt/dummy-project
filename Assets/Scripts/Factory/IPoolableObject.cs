using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Contract for objects managed by a pool, exposing lifecycle hooks and a backing <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>
    /// Typical implementations toggle active state and reset transient data in <see cref="OnRent"/> / <see cref="OnRelease"/>.
    /// </remarks>
    public interface IPoolableObject
    {
        /// <summary>
        /// Called immediately after the object is rented from the pool.
        /// Use this to activate visuals and reset per-use state.
        /// </summary>
        void OnRent();

        /// <summary>
        /// Called right before the object is returned to the pool.
        /// Use this to stop effects, cancel coroutines, and disable input.
        /// </summary>
        void OnRelease();

        /// <summary>
        /// Backing Unity object for activation/deactivation and transform access.
        /// </summary>
        GameObject GameObject { get; }
    }
}
