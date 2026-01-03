using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltPresenterFactory : IBoltPresenterFactory
    {
        private readonly DiContainer _container;
        private readonly Camera _camera;
        private readonly IBoltTargetingService _targetingService;
        private readonly float _edgePaddingWorld;
        private readonly float _ricochetCooldown;

        public BoltPresenterFactory(
            DiContainer container,
            Camera camera,
            IBoltTargetingService targetingService,
            [Inject(Id = "BoltEdgePaddingWorld", Optional = true)] float edgePaddingWorld = 0.25f,
            [Inject(Id = "BoltRicochetCooldown", Optional = true)] float ricochetCooldown = 0.08f)
        {
            _container = container;
            _camera = camera;
            _targetingService = targetingService;
            _edgePaddingWorld = edgePaddingWorld;
            _ricochetCooldown = ricochetCooldown;
        }

        public BoltPresenter Create(BoltView view, BoltConfig config)
        {
            // Pull stats directly from BoltConfig
            string name = config ? config.DisplayName : "Bolt";
            Sprite sprite = config ? config.Sprite : null;
            int damage = config ? config.Damage : 1;
            float speed = config ? config.Speed : 12f;
            float life = config ? config.LifetimeSeconds : 6f;

            var model = _container.Instantiate<BoltModel>(new object[] { name, sprite, damage, speed, life });

            var presenter = _container.Instantiate<BoltPresenter>(new object[]
            {
                (IBoltModel)model,
                view,
                _camera,
                _targetingService,
                _edgePaddingWorld,
                _ricochetCooldown
            });

            presenter.Initialize();
            return presenter;
        }
    }
}
