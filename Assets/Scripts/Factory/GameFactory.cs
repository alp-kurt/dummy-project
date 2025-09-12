using Zenject;

namespace Scripts
{
    public sealed class GameFactory
    {
        private readonly DiContainer _container;

        public GameFactory(DiContainer container) => _container = container;

        // ENEMY ---------------------------------------------------------------

        public EnemyPresenter CreateEnemy(EnemyView view, EnemyStats stats)
        {
            return _container.Instantiate<EnemyPresenter>(new object[] { view, stats });
        }

        // PROJECTILE  ---------------------------------------------

        public ProjectilePresenter CreateProjectile(ProjectileView view, IProjectileModel model)
        {
            return _container.Instantiate<ProjectilePresenter>(new object[] { model, view });
        }
    }
}
