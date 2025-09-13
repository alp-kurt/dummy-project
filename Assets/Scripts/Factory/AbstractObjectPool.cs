using System;
using System.Collections.Generic;

namespace Scripts
{
    public class ObjectPool<T> : IObjectPool<T> where T : IPoolableObject
    {
        private readonly Queue<T> m_available = new Queue<T>();
        private readonly HashSet<T> m_inUse = new HashSet<T>();
        private readonly IObjectFactory<T> m_factory;
        private readonly int m_min;
        private readonly int m_max;

        public ObjectPool(IObjectFactory<T> factory, int min, int max)
        {
            m_factory = factory ?? throw new ArgumentNullException(nameof(factory));
            m_min = Math.Max(0, min);
            m_max = Math.Max(m_min, max);
            Prewarm(m_min);
        }

        public int AvailableCount => m_available.Count;
        public int InUseCount => m_inUse.Count;

        public T GetObject()
        {
            T instance = m_available.Count > 0 ? m_available.Dequeue() : CreateIfAllowed();
            if (instance == null) throw new InvalidOperationException("Pool depleted and max reached.");

            m_inUse.Add(instance);
            OnRented(instance);
            return instance;
        }

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

        public void ReleaseObject(T obj)
        {
            if (obj == null) return;
            if (!m_inUse.Remove(obj)) return;

            OnReleased(obj);
            m_available.Enqueue(obj);
        }

        // Hooks for engine specifics
        protected virtual void OnCreated(T instance) { instance.GameObject.SetActive(false); }
        protected virtual void OnRented(T instance) { instance.OnRent(); }
        protected virtual void OnReleased(T instance) { instance.OnRelease(); }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var inst = m_factory.CreateNew();
                OnCreated(inst);
                m_available.Enqueue(inst);
            }
        }

        private T CreateIfAllowed()
        {
            if (m_available.Count + m_inUse.Count >= m_max) return default;
            var inst = m_factory.CreateNew();
            OnCreated(inst);
            return inst;
        }
    }
}
