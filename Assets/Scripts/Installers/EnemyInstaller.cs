using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyInstaller : MonoInstaller
    {
        [SerializeField] private WaveConfig m_waveConfig;
        [SerializeField] private EnemyPool m_enemyPoolInScene;

        public override void InstallBindings()
        {
            if (m_waveConfig != null && !Container.HasBinding<WaveConfig>())
                Container.Bind<WaveConfig>().FromInstance(m_waveConfig).AsSingle();

            // Per-enemy models 
            Container.Bind<IEnemyHealthModel>().To<EnemyHealthModel>().AsTransient();
            Container.Bind<IEnemyModel>().To<EnemyModel>().AsTransient();

            // Presenter factory
            Container.BindFactory<EnemyView, EnemyStats, EnemyPresenter, EnemyPresenterFactory>();

            if (m_enemyPoolInScene != null && !Container.HasBinding<EnemyPool>())
                Container.Bind<EnemyPool>().FromInstance(m_enemyPoolInScene).AsSingle();

            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();

        }
    }

    public sealed class EnemyPresenterFactory
        : PlaceholderFactory<EnemyView, EnemyStats, EnemyPresenter>
    { }
}
