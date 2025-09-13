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
            // How to create EnemyView
            Container.Bind<IObjectFactory<EnemyView>>()
                .To<PrefabFactory<EnemyView>>()
                .AsSingle()
                .WithArguments(enemyPrefab, pooledParent);

            // Typed pool that handles parenting on rent/release
            Container.Bind<IObjectPool<EnemyView>>()
                .To<EnemyViewPool>()
                .AsSingle()
                .WithArguments(min, max, pooledParent, activeParent);

            // Enemy composition
            Container.Bind<IEnemyHealthModel>().To<EnemyHealthModel>().AsTransient();
            Container.BindInterfacesAndSelfTo<EnemyModel>().AsTransient();
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();


            if (activeParent != null)
                Container.Bind<Transform>().WithId("EnemyActiveParent").FromInstance(activeParent).AsSingle();
        }
    }
}
