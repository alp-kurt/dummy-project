using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Central installer for Bolt system:
    /// - Pools, renter, presenter factory, IBoltFactory (existing)
    /// - Ricochet tunables
    /// - Spawner MVP (config → model, presenter tick; spawns from IPlayerPosition)
    /// </summary>
    public sealed class BoltSystemInstaller : MonoInstaller
    {
        [Header("Prefabs & Parents")]
        [SerializeField] private BoltView boltPrefab;
        [SerializeField] private Transform pooledParent;
        [SerializeField] private Transform activeParent;

        [Header("Pool Size")]
        [SerializeField, Min(1)] private int min = 8;
        [SerializeField, Min(1)] private int max = 32;

        [Header("Ricochet Config")]
        [SerializeField, Min(0f)] private float edgePaddingWorld = 0.25f;
        [SerializeField, Min(0f)] private float ricochetCooldown = 0.08f;

        [Tooltip("Designer-tunable: bolts per cast & seconds between casts.")]
        [SerializeField] private BoltSpawnerConfig spawnerConfig;

        [Tooltip("Existing projectile stats SO used by bolts created by the spawner.")]
        [SerializeField] private BoltConfig boltConfig;

        public override void InstallBindings()
        {
            // ===== Fail-fast validations =====
            if (!boltPrefab) throw new System.NullReferenceException("[BoltSystemInstaller] Bolt prefab is not assigned.");
            if (!pooledParent) throw new System.NullReferenceException("[BoltSystemInstaller] Pooled parent is not assigned.");
            if (!activeParent) throw new System.NullReferenceException("[BoltSystemInstaller] Active parent is not assigned.");

            if (!spawnerConfig) throw new System.NullReferenceException("[BoltSystemInstaller] Spawner Config is not assigned.");
            if (!boltConfig) throw new System.NullReferenceException("[BoltSystemInstaller] BoltConfig is not assigned.");

            // ===== Core factory & pooling =====
            // IObjectFactory<BoltView> -> PrefabFactory<BoltView> (prefab + pooledParent)
            Container.Bind<IObjectFactory<BoltView>>()
                .To<PrefabFactory<BoltView>>()
                .AsSingle()
                .WithArguments(boltPrefab, pooledParent);

            // IObjectPool<BoltView> -> BoltViewPool (min, max, pooledParent, activeParent)
            Container.Bind<IObjectPool<BoltView>>()
                .To<BoltViewPool>()
                .AsSingle()
                .WithArguments(min, max, pooledParent, activeParent);

            // Renter, PresenterFactory, Factory
            Container.Bind<IBoltViewRenter>().To<BoltViewRenter>().AsSingle();
            Container.Bind<IBoltPresenterFactory>().To<BoltPresenterFactory>().AsSingle();
            Container.Bind<IBoltFactory>().To<BoltFactory>().AsSingle();

            // Ricochet tunables
            Container.Bind<float>().WithId("BoltEdgePaddingWorld").FromInstance(edgePaddingWorld).IfNotBound();
            Container.Bind<float>().WithId("BoltRicochetCooldown").FromInstance(ricochetCooldown).IfNotBound();

            // ===== Spawner MVP =====
            Container.BindInstance(spawnerConfig);
            Container.BindInstance(boltConfig);

            // Model pulls from the config directly
            Container.Bind<IBoltSpawnerModel>()
                     .To<BoltSpawnerModel>()
                     .AsSingle();

            // Presenter depends on IBoltSpawnerModel, IBoltFactory, IPlayerPosition, BoltSpawnerConfig, BoltConfig
            Container.BindInterfacesTo<BoltSpawnerPresenter>()
                     .AsSingle()
                     .NonLazy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (min < 1) min = 1;
            if (max < min) max = min;

            if (!boltPrefab) Debug.LogWarning("[BoltSystemInstaller] Bolt prefab missing.", this);
            if (!pooledParent) Debug.LogWarning("[BoltSystemInstaller] Pooled parent missing.", this);
            if (!activeParent) Debug.LogWarning("[BoltSystemInstaller] Active parent missing.", this);

            if (!spawnerConfig) Debug.LogWarning("[BoltSystemInstaller] Spawner Config missing.", this);
            if (!boltConfig) Debug.LogWarning("[BoltSystemInstaller] BoltConfig missing.", this);
        }
#endif
    }
}
