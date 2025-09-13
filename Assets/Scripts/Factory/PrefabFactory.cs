using UnityEngine;

namespace Scripts
{
    public sealed class PrefabFactory<T> : IObjectFactory<T> where T : Component
    {
        private readonly T m_prefab;
        private readonly Transform m_parent;

        public PrefabFactory(T prefab, Transform parent)
        {
            m_prefab = prefab;
            m_parent = parent;
        }

        public T CreateNew()
        {
            return Object.Instantiate(m_prefab, m_parent);
        }
    }
}
