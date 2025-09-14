using Zenject;
using UnityEngine; // for Debug.LogWarning

namespace Scripts
{
    public sealed class EnemyPresenterFactory : IEnemyPresenterFactory
    {
        private readonly DiContainer m_container;

        public EnemyPresenterFactory(DiContainer container)
        {
            m_container = container;
        }

        public EnemyPresenter Create(EnemyView view, EnemyStats stats)
        {
            // Create per-enemy instances explicitly
            var model = m_container.Instantiate<EnemyModel>();
            var health = m_container.Instantiate<EnemyHealthModel>(); // single source of truth

            // Wire the same health instance to the damageable adapter (once per spawn)
            var adapter = view.GetComponent<EnemyDamageableAdapter>();
            if (adapter != null)
            {
                adapter.Initialize(health);
            }

            // Cross-cutting deps
            var player = m_container.Resolve<PlayerView>();
            var deathBus = m_container.Resolve<IEnemyDeathStream>();

            // Pass the same health instance to the presenter
            return m_container.Instantiate<EnemyPresenter>(
                new object[] { model, health, player, view, stats, deathBus });
        }
    }
}