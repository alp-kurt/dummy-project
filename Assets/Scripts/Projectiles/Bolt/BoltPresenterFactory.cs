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
            // BoltModel(string name, Sprite sprite, ProjectileDamage dmg, ProjectileSpeed spd)  :contentReference[oaicite:2]{index=2}
            var damage = new ProjectileDamage(config.BaseDamage);   // 
            var speed = new ProjectileSpeed(config.BaseSpeed);     // 

            var model = m_container.Instantiate<BoltModel>(
                new object[] { config.DisplayName, config.Sprite, damage, speed } // :contentReference[oaicite:5]{index=5}
            );

            // The presenter signature is (IBoltModel, ProjectileView).  :contentReference[oaicite:6]{index=6}
            // Ensure BoltView is (or derives from) ProjectileView; see note above.
            var presenter = m_container.Instantiate<BoltPresenter>(
                new object[] { (IBoltModel)model, (ProjectileView)view }
            );

            presenter.Initialize(); // wires up model ↔ view + movement tick. :contentReference[oaicite:7]{index=7}
            return presenter;
        }
    }
}
