using System;
using System.Collections.Generic;

namespace Scripts
{
    /// <summary>
    /// A bounded queue-based pool for <typeparamref name="T"/> with prewarming and lifecycle hooks.
    /// </summary>
    /// <typeparam name="T">
    /// Pooled type. Must implement <see cref="IPoolableObject"/>.
    /// In this project, pooled types are usually Unity <c>Component</c>s as well.
    /// </typeparam>
    /// <remarks>
    /// Design:
    /// <list type="bullet">
    /// <item><description>Prewarms <c>min</c> instances on construction to avoid runtime spikes.</description></item>
    /// <item><description>Honors a hard <c>max</c> capacity to prevent runaway allocations.</description></item>
    /// <item><description>Exposes overridable hooks for engine-specific behaviors.</description></item>
    /// </list>
    /// Thread-safety: not thread-safe by design (Unity main-thread usage).
    /// </remarks>
    public class ObjectPool<T> : IObjectPool<T> where T : IPoolableObject
    {
        private readonly Queue<T> m_available = new Queue<T>();
        private readonly HashSet<T> m_inUse = new HashSet<T>();
        private readonly IObjectFactory<T> m_factory;
        private readonly int m_min;
        private readonly int m_max;

        /// <summary>
        /// Creates a new pool and prewarms it with <paramref name="min"/> instances.
        /// </summary>
        /// <param name="factory">Factory used to create brand-new instances.</param>
        /// <param name="min">Minimum prewarmed instances (clamped to ≥ 0).</param>
        /// <param name="max">Maximum capacity (clamped to ≥ <paramref name="min"/>).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factory"/> is null.</exception>
        public ObjectPool(IObjectFactory<T> factory, int min, int max)
        {
            m_factory = factory ?? throw new ArgumentNullException(nameof(factory));
            m_min = Math.Max(0, min);
            m_max = Math.Max(m_min, max);
            Prewarm(m_min);
        }

        /// <inheritdoc/>
        public int AvailableCount => m_available.Count;

        /// <inheritdoc/>
        public int InUseCount => m_inUse.Count;

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no instances are available and the pool is at maximum capacity.
        /// </exception>
        public T GetObject()
        {
            T instance = m_available.Count > 0 ? m_available.Dequeue() : CreateIfAllowed();
            if (instance == null) throw new InvalidOperationException("Pool depleted and max reached.");

            m_inUse.Add(instance);
            OnRented(instance);
            return instance;
        }

        /// <inheritdoc/>
        public bool TryGetObject(out T instance)
        {
            if (m_available.Count == 0 && (m_available.Count + m_inUse.Count) >= m_max)
            {
                instance = default;
                return false;
            }

            instance = (m_available.Count > 0) ? m_available.Dequeue() : CreateIfAllowed();
            if (instance == null) return false;

            m_inUse.Add(instance);
            OnRented(instance);
            return true;
        }

        /// <inheritdoc/>
        public void ReleaseObject(T obj)
        {
            if (obj == null) return;
            if (!m_inUse.Remove(obj)) return; // Guard: ignore unknown or duplicate releases.

            OnReleased(obj);
            m_available.Enqueue(obj);
        }

        #region Hooks
        /// <summary>
        /// Called after a new instance is created by the factory, before it is enqueued.
        /// Default: deactivates the underlying <see cref="IPoolableObject.GameObject"/>.
        /// </summary>
        protected virtual void OnCreated(T instance) { instance.GameObject.SetActive(false); }

        /// <summary>
        /// Called immediately after an instance is rented via <see cref="GetObject"/> / <see cref="TryGetObject"/>.
        /// Default: invokes <see cref="IPoolableObject.OnRent"/>.
        /// </summary>
        protected virtual void OnRented(T instance) { instance.OnRent(); }

        /// <summary>
        /// Called right before an instance is enqueued back to the pool.
        /// Default: invokes <see cref="IPoolableObject.OnRelease"/>.
        /// </summary>
        protected virtual void OnReleased(T instance) { instance.OnRelease(); }
        #endregion

        /// <summary>
        /// Allocates and enqueues <paramref name="count"/> fresh instances using the factory.
        /// </summary>
        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var inst = m_factory.CreateNew();
                OnCreated(inst);
                m_available.Enqueue(inst);
            }
        }

        /// <summary>
        /// Creates a new instance if doing so will not exceed <c>max</c>; otherwise returns default.
        /// </summary>
        private T CreateIfAllowed()
        {
            if (m_available.Count + m_inUse.Count >= m_max) return default;
            var inst = m_factory.CreateNew();
            OnCreated(inst);
            return inst;
        }
    }
}
