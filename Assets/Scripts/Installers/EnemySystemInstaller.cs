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
            if (enemyPrefab == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (pooledParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (activeParent == null)
                throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");

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

            // Cross-cutting streams / services
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();

            // Composition
            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            // Note:
            // This system does NOT rely on container injection for IEnemyHealthModel in adapters.
            // EnemyPresenterFactory Instantiates one EnemyHealthModel per enemy and shares it explicitly.
            // If other systems need to Resolve<IEnemyHealthModel>, you can keep a transient binding:
            // Container.Bind<IEnemyHealthModel>().To<EnemyHealthModel>().AsTransient();
            // (Not used by this adapter/presenter path, so it's optional.)
        }
    }
}
