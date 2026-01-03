using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Zenject memory pool for EnemyView with parenting and baseline reset.
    /// </summary>
    public sealed class EnemyViewPool : MonoMemoryPool<EnemyView>
    {
        private Transform _pooledParent;
        private Transform _activeParent;

        [Inject]
        public void Construct(
            [Inject(Id = "PooledEnemiesRoot")] Transform pooledParent,
            [Inject(Id = "ActiveEnemiesRoot")] Transform activeParent
        )
        {
            _pooledParent = pooledParent;
            _activeParent = activeParent;
        }

        protected override void OnSpawned(EnemyView item)
        {
            if (_activeParent != null)
                item.transform.SetParent(_activeParent, false);
            item.OnRent();
        }

        protected override void OnDespawned(EnemyView item)
        {
            item.OnRelease();
            if (_pooledParent != null)
                item.transform.SetParent(_pooledParent, false);
        }
    }
}
