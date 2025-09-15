using Zenject;

namespace Scripts
{
    /// <summary>
    /// Builds a fully wired EnemyPresenter (+ model/health) for a rented EnemyView using the given stats.
    /// Globals (PlayerView, IEnemyDeathStream) are injected once into this factory.
    /// </summary>
    public sealed class EnemyPresenterFactory : IEnemyPresenterFactory
    {
        private readonly DiContainer m_container;
        private readonly PlayerView m_player;
        private readonly IEnemyDeathStream _;

        public EnemyPresenterFactory(
            DiContainer container,
            PlayerView player,
            IEnemyDeathStream deathBus)
        {
            m_container = container;
            m_player = player;
            _ = deathBus;
        }

        public EnemyPresenter Create(EnemyView view, EnemyStats stats)
        {
            // Per-enemy instances
            var model = m_container.Instantiate<EnemyModel>();
            var health = m_container.Instantiate<EnemyHealthModel>();

            // Manual wiring for Unity component adapter (consistent with your hybrid DI style)
            var dmgAdapter = view.GetComponent<EnemyDamageableAdapter>();
            dmgAdapter?.Initialize(health);

            // Construct the presenter with explicit dependencies (no Resolve calls here)
            var presenter = m_container.Instantiate<EnemyPresenter>(new object[]
            {
                model,
                health,
                m_player,
                view,
                stats,
                _
            });

            return presenter;
        }
    }
}
