using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Zenject memory pool for BoltView with parenting and baseline reset.
    /// </summary>
    public sealed class BoltViewPool : MonoMemoryPool<BoltView>
    {
        private Transform _pooledParent;
        private Transform _activeParent;

        [Inject]
        public void Construct(
            [Inject(Id = "PooledBoltsRoot")] Transform pooledParent,
            [Inject(Id = "ActiveBoltsRoot")] Transform activeParent
        )
        {
            _pooledParent = pooledParent;
            _activeParent = activeParent;
        }

        protected override void OnSpawned(BoltView item)
        {
            if (_activeParent != null)
                item.CachedTransform.SetParent(_activeParent, worldPositionStays: false);
            item.OnRent();
        }

        protected override void OnDespawned(BoltView item)
        {
            item.OnRelease();
            if (_pooledParent != null)
                item.CachedTransform.SetParent(_pooledParent, worldPositionStays: false);
        }
    }
}
