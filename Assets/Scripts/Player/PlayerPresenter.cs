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
        private readonly SignalBus _signalBus;

        private readonly TimeSpan _hitCooldown = TimeSpan.FromMilliseconds(300);
        private float _nextHitTime;
        private readonly CompositeDisposable _cd = new();
        public PlayerPresenter(JoystickView joystickView, PlayerView view, IPlayerModel model, SignalBus signalBus)
        {
            _joystickView = joystickView;
            _view = view;
            _model = model;
            _signalBus = signalBus;
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

            _signalBus.Subscribe<PlayerEnemyCollidedSignal>(OnEnemyCollided);
            _signalBus.Subscribe<PlayerDiedSignal>(OnPlayerDied);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerEnemyCollidedSignal>(OnEnemyCollided);
            _signalBus.Unsubscribe<PlayerDiedSignal>(OnPlayerDied);
            _cd.Dispose();
        }

        private void OnEnemyCollided(PlayerEnemyCollidedSignal signal)
        {
            if (Time.time < _nextHitTime) return;
            _nextHitTime = Time.time + (float)_hitCooldown.TotalSeconds;

            int dmg = signal.Damage > 0 ? signal.Damage : 1;
            _model.TakeDamage(dmg);
        }

        private void OnPlayerDied(PlayerDiedSignal signal)
        {
            if (signal.Model != _model) return;
            _model.SetMoveInput(Vector2.zero);
            Debug.Log("[Player] Died");
        }
    }
}
