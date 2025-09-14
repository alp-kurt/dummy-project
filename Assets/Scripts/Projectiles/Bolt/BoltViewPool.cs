using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Typed pool that manages parenting only. Active/inactive is driven by PooledView hooks.
    /// Mirrors your EnemyViewPool pattern.
    /// </summary>
    public sealed class BoltViewPool : ObjectPool<BoltView>
    {
        private readonly Transform m_pooledParent;
        private readonly Transform m_activeParent;

        public BoltViewPool(
            IObjectFactory<BoltView> factory,
            int min,
            int max,
            Transform pooledParent,
            Transform activeParent
        ) : base(factory, min, max)
        {
            m_pooledParent = pooledParent;
            m_activeParent = activeParent;
        }

        protected override void OnCreated(BoltView view)
        {
            base.OnCreated(view);
            if (m_pooledParent != null)
                view.CachedTransform.SetParent(m_pooledParent, worldPositionStays: false);
        }

        protected override void OnRented(BoltView view)
        {
            base.OnRented(view);
            if (m_activeParent != null)
                view.CachedTransform.SetParent(m_activeParent, worldPositionStays: true);
            view.OnRent(); // Provided by PooledView
        }

        protected override void OnReleased(BoltView view)
        {
            base.OnReleased(view);
            if (m_pooledParent != null)
                view.CachedTransform.SetParent(m_pooledParent, worldPositionStays: false);
            view.OnRelease(); // Provided by PooledView
        }
    }
}
