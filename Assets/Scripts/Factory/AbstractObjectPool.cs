using System;
using System.Collections.Generic;
using UniRx;

namespace Scripts
{
    public class AbstractObjectPool<T> : IObjectPool<T> where T : IPoolableObject
    {
        private readonly Queue<T> m_available = new Queue<T>();
        private readonly HashSet<T> m_inUse = new HashSet<T>();
        private readonly IObjectFactory<T> m_factory;
        private readonly int m_min;
        private readonly int m_max;

        private readonly Subject<T> m_rented = new Subject<T>();
        private readonly Subject<T> m_released = new Subject<T>();

        public AbstractObjectPool(IObjectFactory<T> factory, int min, int max)
        {
            m_factory = factory ?? throw new ArgumentNullException(nameof(factory));
            m_min = Math.Max(0, min);
            m_max = Math.Max(m_min, max);
            Prewarm(m_min);
        }

        public int AvailableCount => m_available.Count;
        public int InUseCount => m_inUse.Count;

        public IObservable<T> Rented => m_rented;
        public IObservable<T> Released => m_released;

        public T GetObject()
        {
            T instance = m_available.Count > 0 ? m_available.Dequeue() : CreateIfAllowed();
            if (instance == null) throw new InvalidOperationException("Pool depleted and max reached.");

            m_inUse.Add(instance);
            OnRented(instance);
            m_rented.OnNext(instance);
            return instance;
        }

        public void ReleaseObject(T obj)
        {
            if (obj == null) return;
            if (!m_inUse.Remove(obj)) return;

            OnReleased(obj);
            m_available.Enqueue(obj);
            m_released.OnNext(obj);
        }

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
