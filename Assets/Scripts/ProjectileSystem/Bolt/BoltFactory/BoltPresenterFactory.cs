using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Creates a BoltPresenter + BoltModel pair and wires scene deps (Camera, ActiveEnemiesRoot).
    /// Expects:
    ///  - Camera bound once (PlayerInstaller)
    ///  - Transform with Id "ActiveEnemiesRoot" bound once (EnemySystemInstaller)
    ///  - Optional float Ids "BoltEdgePaddingWorld" / "BoltRicochetCooldown" (BoltSystemInstaller)
    /// </summary>
    public sealed class BoltPresenterFactory : IBoltPresenterFactory
    {
        private readonly DiContainer _container;
        private readonly Camera _camera; // from PlayerInstaller
        private readonly Transform _activeEnemiesRoot; // from EnemySystemInstaller
        private readonly float _edgePaddingWorld;
        private readonly float _ricochetCooldown;

        public BoltPresenterFactory(
            DiContainer container,
            Camera camera,
            [Inject(Id = "ActiveEnemiesRoot")] Transform activeEnemiesRoot,
            [Inject(Id = "BoltEdgePaddingWorld", Optional = true)] float edgePaddingWorld = 0.25f,
            [Inject(Id = "BoltRicochetCooldown", Optional = true)] float ricochetCooldown = 0.08f)
        {
            _container = container;
            _camera = camera;
            _activeEnemiesRoot = activeEnemiesRoot;
            _edgePaddingWorld = edgePaddingWorld;
            _ricochetCooldown = ricochetCooldown;
        }

        public BoltPresenter Create(BoltView view, ProjectileConfigBase config)
        {
            // Build model from config
            var damage = new ProjectileDamage(config.BaseDamage);
            var speed = new ProjectileSpeed(config.BaseSpeed);

            var boltCfg = (BoltConfig)config;
            var model = _container.Instantiate<BoltModel>(
                new object[]
                {
                    config.DisplayName,
                    config.Sprite,
                    damage,
                    speed,
                    boltCfg.LifetimeSeconds
                }
            );

            // Build presenter with required scene deps and tunables
            var presenter = _container.Instantiate<BoltPresenter>(
                new object[]
                {
                    (IBoltModel)model,
                    (ProjectileView)view,
                    _camera,
                    _activeEnemiesRoot,
                    _edgePaddingWorld,
                    _ricochetCooldown
                }
            );

            presenter.Initialize();
            return presenter;
        }
    }
}
