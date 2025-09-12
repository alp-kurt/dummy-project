using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyInstaller : MonoInstaller
    {
        [Header("Optional scene-level references")]
        [SerializeField] private WaveConfig waveConfig;
        [SerializeField] private EnemyPool enemyPool;

        public override void InstallBindings()
        {
            // Per-enemy models (created per factory call)
            Container.BindInterfacesTo<EnemyHealthModel>().AsTransient();
            Container.BindInterfacesTo<EnemyModel>().AsTransient();

            // Presenter factory (view + stats are supplied at Create(...))
            Container.BindFactory<EnemyView, EnemyStats, EnemyPresenter, EnemyPresenterFactory>();

            // Optional scene-level singletons (bind only if assigned)
            if (waveConfig != null)
                Container.Bind<WaveConfig>().FromInstance(waveConfig).AsSingle();

            if (enemyPool != null)
                Container.Bind<EnemyPool>().FromInstance(enemyPool).AsSingle();
        }
    }

    public sealed class EnemyPresenterFactory
        : PlaceholderFactory<EnemyView, EnemyStats, EnemyPresenter>
    { }
}
