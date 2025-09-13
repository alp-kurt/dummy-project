using Zenject;

namespace Scripts
{
    /// <summary>
    /// Centralizes construction of EnemyPresenter (and the EnemyModel it depends on).
    /// Keeps EnemyFactory tiny and testable.
    /// </summary>
    public sealed class EnemyPresenterFactory : IEnemyPresenterFactory
    {
        private readonly DiContainer m_container;

        public EnemyPresenterFactory(DiContainer container)
        {
            m_container = container;
        }

        public EnemyPresenter Create(EnemyView view, EnemyStats stats)
        {
            // Model via DI (so EnemyModel gets its own ctor dependencies, if any)
            var model = m_container.Instantiate<EnemyModel>();

            // Per-enemy dependencies (already bound in your installers)
            var health = m_container.Resolve<IEnemyHealthModel>();
            var player = m_container.Resolve<PlayerView>();
            var deathBus = m_container.Resolve<IEnemyDeathStream>();

            // Your existing EnemyPresenter signature:
            return m_container.Instantiate<EnemyPresenter>(
                new object[] { model, health, player, view, stats, deathBus });
        }
    }
}
