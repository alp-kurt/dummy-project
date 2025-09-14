using UnityEngine;

namespace Scripts
{
    public sealed class BoltViewRenter : IBoltViewRenter
    {
        private readonly IObjectPool<BoltView> m_pool;

        public BoltViewRenter(IObjectPool<BoltView> pool)
        {
            m_pool = pool;
        }

        public BoltView Rent(Vector3 worldPosition)
        {
            var view = m_pool.GetObject();
            view.CachedTransform.position = worldPosition;
            return view;
        }

        public void Return(BoltView view)
        {
            if (view == null) return;
            m_pool.ReleaseObject(view);
        }
    }
}