namespace Scripts
{
    /// <summary>
    /// Abstracts creation of new instances of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type produced by the factory.</typeparam>
    public interface IObjectFactory<T>
    {
        /// <summary>
        /// Creates a brand new instance.
        /// </summary>
        /// <remarks>
        /// Implementations should not return cached objects; use pooling for reuse.
        /// </remarks>
        T CreateNew();
    }
}
