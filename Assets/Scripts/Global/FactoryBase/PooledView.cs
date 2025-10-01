using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Convenience base <see cref="MonoBehaviour"/> for pooled Unity objects.
    /// Implements default activation/deactivation semantics.
    /// </summary>
    public abstract class PooledView : MonoBehaviour, IPoolableObject
    {
        /// <inheritdoc/>
        public GameObject GameObject => gameObject;

        /// <summary>
        /// Default rent behavior: activates the GameObject.
        /// Override to reset transient state (e.g., HP, timers, velocity).
        /// </summary>
        public virtual void OnRent()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Default release behavior: deactivates the GameObject.
        /// Override to stop effects or cancel coroutines.
        /// </summary>
        public virtual void OnRelease()
        {
            gameObject.SetActive(false);
        }
    }
}
