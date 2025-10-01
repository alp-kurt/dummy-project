using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltSystemInstaller : MonoInstaller
    {
        [Header("Prefabs & Parents")]
        [SerializeField] private BoltView _boltPrefab;
        [SerializeField] private Transform _pooledParent;
        [SerializeField] private Transform _activeParent;

        [Header("Pool Size")]
        [SerializeField, Min(1)] private int _min = 8;
        [SerializeField, Min(1)] private int _max = 32;

        [Header("Ricochet Config")]
        [SerializeField, Min(0f)] private float _edgePaddingWorld = 0.25f;
        [SerializeField, Min(0f)] private float _ricochetCooldown = 0.08f;

        [Header("Bolt Config (REQUIRED)")]
        [SerializeField] private BoltConfig _boltConfig;

        public override void InstallBindings()
        {
            // Validation
            if (!_boltPrefab) throw new System.NullReferenceException("[BoltSystemInstaller] Bolt prefab is not assigned.");
            if (!_pooledParent) throw new System.NullReferenceException("[BoltSystemInstaller] Pooled parent is not assigned.");
            if (!_activeParent) throw new System.NullReferenceException("[BoltSystemInstaller] Active parent is not assigned.");
            if (_max < _min) throw new System.Exception($"[BoltSystemInstaller] max ({_max}) < min ({_min}).");
            if (!_boltConfig) throw new System.NullReferenceException("[BoltSystemInstaller] BoltConfig is not assigned.");

            // Factory + Pool
            Container.Bind<IObjectFactory<BoltView>>()
                     .To<PrefabFactory<BoltView>>()
                     .AsSingle()
                     .WithArguments(_boltPrefab, _pooledParent);

            Container.Bind<IObjectPool<BoltView>>()
                     .To<BoltViewPool>()
                     .AsSingle()
                     .WithArguments(_min, _max, _pooledParent, _activeParent);

            Container.Bind<IBoltViewRenter>().To<BoltViewRenter>().AsSingle();
            Container.Bind<IBoltPresenterFactory>().To<BoltPresenterFactory>().AsSingle();
            Container.Bind<IBoltFactory>().To<BoltFactory>().AsSingle();

            // Tunables for presenter factory
            Container.Bind<float>().WithId("BoltEdgePaddingWorld").FromInstance(_edgePaddingWorld).IfNotBound();
            Container.Bind<float>().WithId("BoltRicochetCooldown").FromInstance(_ricochetCooldown).IfNotBound();

            // Bind the goddamn BoltConfig
            Container.Bind<BoltConfig>().FromInstance(_boltConfig).AsSingle();

            // Initialize in-scene BoltSpawner (MonoBehaviour)
            Container.BindInterfacesAndSelfTo<BoltSpawner>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_min < 1) _min = 1;
            if (_max < _min) _max = _min;

            if (!_boltPrefab) Debug.LogWarning("[BoltSystemInstaller] Bolt prefab missing.", this);
            if (!_pooledParent) Debug.LogWarning("[BoltSystemInstaller] Pooled parent missing.", this);
            if (!_activeParent) Debug.LogWarning("[BoltSystemInstaller] Active parent missing.", this);
            if (!_boltConfig) Debug.LogWarning("[BoltSystemInstaller] BoltConfig not assigned.", this);
        }
#endif
    }
}
