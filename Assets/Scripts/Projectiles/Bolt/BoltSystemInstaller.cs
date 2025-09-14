using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltSystemInstaller : MonoInstaller
    {
        [Header("Prefabs & Parents")]
        [SerializeField] private BoltView boltPrefab;
        [SerializeField] private Transform pooledParent;
        [SerializeField] private Transform activeParent;

        [Header("Pool Size")]
        [SerializeField] private int min = 8;
        [SerializeField] private int max = 32;

        public override void InstallBindings()
        {
            // Fail-fast checks
            if (boltPrefab == null)
            {
                throw new System.NullReferenceException("[BoltSystemInstaller] Bolt prefab is not assigned.");
            }
            if (pooledParent == null)
            {
                throw new System.NullReferenceException("[BoltSystemInstaller] Pooled parent is not assigned.");
            }
            if (activeParent == null)
            {
                throw new System.NullReferenceException("[BoltSystemInstaller] Active parent is not assigned.");
            }

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
        }
    }
}
