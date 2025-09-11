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

            // Per-enemy hub + model
            Container.Bind<EnemyEvents>().AsTransient();
            Container.Bind<IEnemyModel>().To<EnemyModel>().AsTransient();

            // Presenter factory (view + stats)
            Container.BindFactory<EnemyView, EnemyStats, EnemyPresenter, EnemyPresenterFactory>();

            if (m_enemyPoolInScene != null && !Container.HasBinding<EnemyPool>())
                Container.Bind<EnemyPool>().FromInstance(m_enemyPoolInScene).AsSingle();

            // (Optional) UI/SFX/Analytics loggers
            Container.Bind<IEnemyUiService>().To<EnemyUiLogger>().AsSingle();
            Container.Bind<IEnemySfxService>().To<EnemySfxLogger>().AsSingle();
            Container.Bind<IEnemyAnalyticsService>().To<EnemyAnalyticsLogger>().AsSingle();
        }
    }

    public sealed class EnemyPresenterFactory
        : PlaceholderFactory<EnemyView, EnemyStats, EnemyPresenter>
    { }
}
