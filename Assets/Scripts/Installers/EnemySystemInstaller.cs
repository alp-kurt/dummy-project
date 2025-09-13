using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemySystemInstaller : MonoInstaller
    {
        [Header("Enemy View Setup")]
        [SerializeField] private EnemyView enemyPrefab;
        [SerializeField] private Transform pooledParent;
        [SerializeField] private Transform activeParent;

        [Header("Pool Size")]
        [SerializeField, Min(0)] private int min = 16;
        [SerializeField, Min(1)] private int max = 128;

        public override void InstallBindings()
        {
            // Defensive checks
            if (enemyPrefab == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (pooledParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (activeParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");

            // --- View creation for the pool
            Container.Bind<IObjectFactory<EnemyView>>()
                .To<PrefabFactory<EnemyView>>()
                .AsSingle()
                .WithArguments(enemyPrefab, pooledParent);

            // --- Typed pool
            // EnemyViewPool should inherit ObjectPool<EnemyView>
            Container.Bind<IObjectPool<EnemyView>>()
                .To<EnemyViewPool>()
                .AsSingle()
                .WithArguments(min, max, pooledParent, activeParent);

            // --- Per-enemy model deps
            Container.Bind<IEnemyHealthModel>().To<EnemyHealthModel>().AsTransient();
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();

            // --- Composition & orchestration
            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
        }
    }
}
