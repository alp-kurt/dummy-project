using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Typed pool that manages parenting only. Active/inactive is driven by PooledView hooks.
    /// </summary>
    public sealed class BoltViewPool : ObjectPool<BoltView>
    {
        private readonly Transform _pooledParent;
        private readonly Transform _activeParent;

        public BoltViewPool(
            IObjectFactory<BoltView> factory,
            int min,
            int max,
            Transform pooledParent,
            Transform activeParent
        ) : base(factory, min, max)
        {
            _pooledParent = pooledParent;
            _activeParent = activeParent;
        }

        protected override void OnCreated(BoltView view)
        {
            base.OnCreated(view);
            if (_pooledParent != null)
                view.CachedTransform.SetParent(_pooledParent, worldPositionStays: false);
        }

        protected override void OnRented(BoltView view)
        {
            base.OnRented(view);
            if (_activeParent != null)
                view.CachedTransform.SetParent(_activeParent, worldPositionStays: true);
            view.OnRent();
        }

        protected override void OnReleased(BoltView view)
        {
            base.OnReleased(view);
            if (_pooledParent != null)
                view.CachedTransform.SetParent(_pooledParent, worldPositionStays: false);
            view.OnRelease();
        }
    }
}
