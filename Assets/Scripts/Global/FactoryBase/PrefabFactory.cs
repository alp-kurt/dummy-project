using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Factory that instantiates a prefab of <typeparamref name="T"/> under an optional parent.
    /// </summary>
    /// <typeparam name="T">
    /// Unity component type that also implements <see cref="IPoolableObject"/> to integrate with pooling.
    /// </typeparam>
    public sealed class PrefabFactory<T> : IObjectFactory<T> where T : Component, IPoolableObject
    {
        private readonly T _prefab;
        private readonly Transform _parent;

        /// <summary>
        /// Binds a prefab and parent transform for subsequent instantiation.
        /// </summary>
        /// <param name="prefab">Prefab used to create new instances.</param>
        /// <param name="parent">Optional parent transform for spawned instances.</param>
        public PrefabFactory(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        /// <summary>
        /// Instantiates a new instance of <typeparamref name="T"/> under the configured parent.
        /// </summary>
        public T CreateNew()
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }
}
