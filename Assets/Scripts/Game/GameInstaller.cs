using Zenject;
using UnityEngine;

namespace Scripts
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField] private Transform activeEnemiesRoot;
       
        public override void InstallBindings()
        {
            Container.Bind<Transform>()
                   .WithId("ActiveEnemiesRoot")
                   .FromInstance(activeEnemiesRoot)
                   .AsSingle();
        }
    }
}
