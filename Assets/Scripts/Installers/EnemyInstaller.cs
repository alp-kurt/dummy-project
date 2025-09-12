using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyInstaller : MonoInstaller
    {
        [Header("Optional scene-level references")]
        [SerializeField] private WaveConfig waveConfig;
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private EnemySpawner enemySpawner;

        public override void InstallBindings()
        {
            if (waveConfig) Container.Bind<WaveConfig>().FromInstance(waveConfig).AsSingle();
            if (enemyPool) Container.Bind<EnemyPool>().FromInstance(enemyPool).AsSingle();

            Container.BindInterfacesTo<EnemyHealthModel>().AsTransient();
            Container.BindInterfacesTo<EnemyModel>().AsTransient();

            Container.Bind<IEnemySpawner>().FromInstance(enemySpawner).AsSingle();
        }
    }

}
