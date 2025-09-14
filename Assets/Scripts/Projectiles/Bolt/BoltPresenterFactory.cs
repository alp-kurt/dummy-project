using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltPresenterFactory : IBoltPresenterFactory
    {
        private readonly DiContainer m_container;

        public BoltPresenterFactory(DiContainer container)
        {
            m_container = container;
        }

        public BoltPresenter Create(BoltView view, ProjectileConfig config)
        {
            // Build the BoltModel inside this factory (no separate model factory).
            // Signatures from your repo:
            // BoltModel(string name, Sprite sprite, ProjectileDamage dmg, ProjectileSpeed spd) 
            var damage = new ProjectileDamage(config.BaseDamage);
            var speed = new ProjectileSpeed(config.BaseSpeed);

            var boltCfg = (BoltConfig)config;
            var model = m_container.Instantiate<BoltModel>(
                new object[] { config.DisplayName, config.Sprite, damage, speed, boltCfg.LifetimeSeconds }
            );

            // The presenter signature is (IBoltModel, ProjectileView).
            // Ensure BoltView is (or derives from) ProjectileView; see note above.
            var presenter = m_container.Instantiate<BoltPresenter>(
            new object[] { (IBoltModel)model, (ProjectileView)view }
            );

            presenter.Initialize();
            return presenter;
        }
    }
}
