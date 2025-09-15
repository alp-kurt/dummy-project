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
            // ---- Validation ----
            if (enemyPrefab == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (pooledParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (activeParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");

            // ---- Cross-cutting streams/services ----
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();

            // ---- Scene singleton: ActiveEnemiesRoot (use activeParent) ----
            Container.Bind<Transform>()
                     .WithId("ActiveEnemiesRoot")
                     .FromInstance(activeParent)
                     .AsSingle();

            // ---- Enemy view factory + pool ----
            Container.Bind<IObjectFactory<EnemyView>>()
                .To<PrefabFactory<EnemyView>>()
                .AsSingle()
                .WithArguments(enemyPrefab, pooledParent);

            Container.Bind<IObjectPool<EnemyView>>()
                .To<EnemyViewPool>()
                .AsSingle()
                .WithArguments(min, max, pooledParent, activeParent);

            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            // ---- Wave Spawner MVP ----
            if (spawnerView)
                Container.Bind<EnemyWaveSpawnerView>().FromInstance(spawnerView).AsSingle();
            else
                Container.Bind<EnemyWaveSpawnerView>().FromComponentInHierarchy().AsSingle();

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

            Container.BindInterfacesAndSelfTo<EnemyWaveSpawnerPresenter>().AsSingle().NonLazy();
        }
    }
}
