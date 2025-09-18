using Zenject;

namespace Scripts
{
    /// <summary>
    /// Builds a fully wired EnemyPresenter for a rented EnemyView using the given stats.
    /// No EnemyHealthModel or DamageableAdapter, presenter handles wiring.
    /// </summary>
    public sealed class EnemyPresenterFactory : IEnemyPresenterFactory
    {
        private readonly DiContainer _container;
        private readonly PlayerView _player;

        public EnemyPresenterFactory(DiContainer container, PlayerView player)
        {
            _container = container;
            _player = player;
        }

        public EnemyPresenter Create(EnemyView view, EnemyStats stats)
        {
            // Per-enemy model
            var model = _container.Instantiate<EnemyModel>();

            // Presenter wires: model + player + view + stats + death bus
            var presenter = _container.Instantiate<EnemyPresenter>(new object[]
            {
                model,
                _player,
                view,
                stats,
            });

            return presenter;
        }
    }
}
