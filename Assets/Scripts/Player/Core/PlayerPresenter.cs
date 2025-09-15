using System;
using UnityEngine;
using Zenject;
using UniRx;

namespace Scripts
{
    public sealed class PlayerPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly JoystickView _joystickView;
        private readonly PlayerView _view;
        private readonly IPlayerModel _model;
        private readonly IPlayerHealthModel _health;

        private readonly TimeSpan _hitCooldown = TimeSpan.FromMilliseconds(350);
        private float _nextHitTime;
        private bool _disposed;

        private IDisposable _inputSub;
        private Action<EnemyView> _onEnemyCollidedHandler;

        public PlayerPresenter(JoystickView joystickView, PlayerView view, IPlayerModel model, IPlayerHealthModel health)
        {
            _joystickView = joystickView;
            _view = view;
            _model = model;
            _health = health;
        }

        public void Initialize()
        {
            _inputSub = _joystickView.OnInput
                .DistinctUntilChanged()
                .Subscribe(_model.SetMoveInput);

            _onEnemyCollidedHandler = e =>
            {
                if (Time.time < _nextHitTime) return;
                _nextHitTime = Time.time + (float)_hitCooldown.TotalSeconds;

                var dmg = (e != null && e.ContactDamage > 0) ? e.ContactDamage : 1;
                _health.ReceiveDamage(dmg);
            };

            _view.OnEnemyCollided += _onEnemyCollidedHandler;
        }

        public void Tick()
        {
            var delta = _model.Step(Time.deltaTime);
            _view.Translate(delta);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _inputSub?.Dispose();

            if (_onEnemyCollidedHandler != null)
            {
                _view.OnEnemyCollided -= _onEnemyCollidedHandler;
                _onEnemyCollidedHandler = null;
            }

            _disposed = true;
        }
    }
}
