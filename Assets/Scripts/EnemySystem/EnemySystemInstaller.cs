using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemySystemInstaller : MonoInstaller
    {
        [Header("Enemy View / Pool")]
        [SerializeField] private EnemyView enemyPrefab;
        [SerializeField] private Transform pooledParent;
        [SerializeField] private Transform activeParent;
        [SerializeField, Min(0)] private int min = 16;
        [SerializeField, Min(1)] private int max = 128;

        [Header("Spawner (optional override)")]
        [Tooltip("If left null, the view will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private EnemyWaveSpawnerView spawnerView;

        public override void InstallBindings()
        {
            // ---- Validation for pooling setup ----
            if (enemyPrefab == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (pooledParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (activeParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");

            // ---- Scene singletons / globals ----
            // Cross-cutting streams / services
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();


            // ---- Enemy view factory + pool ----
            // Factory for EnemyView instances (parent under pooledParent on creation)
            Container.Bind<IObjectFactory<EnemyView>>()
                .To<PrefabFactory<EnemyView>>()
                .AsSingle()
                .WithArguments(enemyPrefab, pooledParent);

            // Typed pool for EnemyView
            Container.Bind<IObjectPool<EnemyView>>()
                .To<EnemyViewPool>()
                .AsSingle()
                .WithArguments(min, max, pooledParent, activeParent);

            // Composition for enemy creation
            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            // ---- Wave Spawner MVP (merged from EnemyWaveSpawnerInstaller) ----
            // Bind the view (explicit instance or lazy from hierarchy)
            if (spawnerView)
            {
                Container.Bind<EnemyWaveSpawnerView>().FromInstance(spawnerView).AsSingle();
            }
            else
            {
                Container.Bind<EnemyWaveSpawnerView>().FromComponentInHierarchy().AsSingle();
            }

            // Build model from bound view
            Container.Bind<IEnemyWaveSpawnerModel>()
                .FromMethod(ctx =>
                {
                    var v = ctx.Container.Resolve<EnemyWaveSpawnerView>();
                    var w = v.Wave;
                    if (w == null)
                        throw new System.Exception("EnemySystemInstaller: WaveConfig is not assigned on EnemyWaveSpawnerView.");
                    return new EnemyWaveSpawnerModel(w, v.RandomSeed);
                })
                .AsSingle();

            // Spawner presenter drives the loop; NonLazy to surface issues early
            Container.BindInterfacesAndSelfTo<EnemyWaveSpawnerPresenter>().AsSingle().NonLazy();

            // NOTE:
            // If any adapters/components need IEnemyHealthModel from DI instead of being initialized manually,
            // you could also add: Container.Bind<IEnemyHealthModel>().To<EnemyHealthModel>().AsTransient();
            // (Not required for the current adapter flow; EnemyPresenterFactory initializes the adapter directly.)
        }
    }
}
