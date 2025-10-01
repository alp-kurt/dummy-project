using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Typed pool that re-parents EnemyView under pooled/active roots automatically.
    /// </summary>
    public sealed class EnemyViewPool : ObjectPool<EnemyView>
    {
        private readonly Transform _pooledParent;
        private readonly Transform _activeParent;

        // DI supplies factory; installer supplies min/max + parents
        public EnemyViewPool(
            IObjectFactory<EnemyView> factory,
            int min, int max,
            Transform pooledParent,
            Transform activeParent
        ) : base(factory, min, max)
        {
            _pooledParent = pooledParent;
            _activeParent = activeParent;
        }

        protected override void OnCreated(EnemyView instance)
        {
            if (_pooledParent != null)
                instance.transform.SetParent(_pooledParent, false);

            base.OnCreated(instance); // default: inactive
        }

        protected override void OnRented(EnemyView instance)
        {
            if (_activeParent != null)
                instance.transform.SetParent(_activeParent, false);

            base.OnRented(instance);  // default: OnRent() activates
        }

        protected override void OnReleased(EnemyView instance)
        {
            base.OnReleased(instance); // default: OnRelease() deactivates
            if (_pooledParent != null)
                instance.transform.SetParent(_pooledParent, false);
        }
    }
}
