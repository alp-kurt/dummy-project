using UnityEngine;

namespace Scripts
{
    public sealed class BoltViewRenter : IBoltViewRenter
    {
        private readonly IObjectPool<BoltView> _pool;

        public BoltViewRenter(IObjectPool<BoltView> pool)
        {
            _pool = pool;
        }

        public BoltView Rent(Vector3 worldPosition)
        {
            var view = _pool.GetObject();
            view.CachedTransform.position = worldPosition;
            return view;
        }

        public void Return(BoltView view)
        {
            if (view == null) return;
            _pool.ReleaseObject(view);
        }
    }
}