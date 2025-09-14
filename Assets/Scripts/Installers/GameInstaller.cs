using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField] private Transform activeEnemiesRoot;
        [SerializeField] private Camera camera;

        public override void InstallBindings()
        {
            Container.Bind<Camera>()
                  .FromComponentInHierarchy()   
                  .AsSingle();

            Container.Bind<Transform>()
       .WithId("ActiveEnemiesRoot")
       .FromInstance(activeEnemiesRoot)
       .AsSingle();
        }
    }
}
