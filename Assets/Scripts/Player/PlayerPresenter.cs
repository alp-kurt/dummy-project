using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerPresenter : IInitializable, IDisposable
    {
        private readonly JoystickView _joystickView;
        private readonly PlayerView _view;
        private readonly IPlayerModel _model;

        private readonly TimeSpan _hitCooldown = TimeSpan.FromMilliseconds(300);
        private float _nextHitTime;
        private readonly CompositeDisposable _cd = new();
        private Action<EnemyView> _onEnemyCollidedHandler;

        public PlayerPresenter(JoystickView joystickView, PlayerView view, IPlayerModel model)
        {
            _joystickView = joystickView;
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            // Input -> model
            _joystickView.OnInput
                .DistinctUntilChanged()
                .Subscribe(_model.SetMoveInput)
                .AddTo(_cd);

            // Reactive movement
            Observable.EveryUpdate()
                .Subscribe(_ => _view.Translate(_model.Step(Time.deltaTime)))
                .AddTo(_cd);

            // Collisions -> damage
            _onEnemyCollidedHandler = e =>
            {
                if (Time.time < _nextHitTime) return;
                _nextHitTime = Time.time + (float)_hitCooldown.TotalSeconds;

                var dmg = (e != null && e.ContactDamage > 0) ? e.ContactDamage : 1f;
                _model.TakeDamage(dmg);
            };
            _view.OnEnemyCollided += _onEnemyCollidedHandler;

            // Death reaction
            _model.Died
                .Take(1)
                .Subscribe(_ =>
                {
                    _model.SetMoveInput(Vector2.zero);
                    Debug.Log("[Player] Died");
                })
                .AddTo(_cd);
        }

        public void Dispose()
        {
            _view.OnEnemyCollided -= _onEnemyCollidedHandler;
            _cd.Dispose();
        }
    }
}
