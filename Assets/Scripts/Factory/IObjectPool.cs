namespace Scripts
{
    /// <summary>
    /// Minimal API for renting and returning pooled instances of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Pooled element type.</typeparam>
    public interface IObjectPool<T>
    {
        /// <summary>
        /// Rents an instance from the pool or creates one if capacity allows.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the pool is depleted and the maximum capacity has been reached.
        /// </exception>
        T GetObject();

        /// <summary>
        /// Attempts to rent an instance from the pool without throwing on exhaustion.
        /// </summary>
        /// <param name="instance">The rented instance if available; otherwise default.</param>
        /// <returns><see langword="true"/> if an instance was obtained; otherwise <see langword="false"/>.</returns>
        bool TryGetObject(out T instance);

        /// <summary>
        /// Returns an instance to the pool. No-op if the instance was not marked as in-use.
        /// </summary>
        /// <param name="obj">The instance to return. Null is ignored.</param>
        void ReleaseObject(T obj);

        /// <summary>Number of currently available (idle) instances.</summary>
        int AvailableCount { get; }

        /// <summary>Number of currently rented instances.</summary>
        int InUseCount { get; }
    }
}
