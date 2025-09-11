using System;
using UniRx;
using Zenject;

namespace Scripts
{
    public sealed class PlayerPresenter : IInitializable
    {
        private readonly JoystickView _joystickView;
        private readonly PlayerView _playerView;
        private readonly IPlayerModel _playerModel;
        private readonly CompositeDisposable _disposer;

        // Collision tuning: avoid rapid multi-hit while staying in contact
        private static readonly TimeSpan k_HitCooldown = TimeSpan.FromMilliseconds(350);

        public PlayerPresenter(
            JoystickView joystickView,
            PlayerView playerView,
            IPlayerModel playerModel,
            CompositeDisposable disposer)
        {
            _joystickView = joystickView;
            _playerView = playerView;
            _playerModel = playerModel;
            _disposer = disposer;
        }

        public void Initialize()
        {
            // Movement
            _joystickView.OnInput
                .Subscribe(_playerView.Move)
                .AddTo(_disposer);

            // Health → UI
            _playerModel.CurrentHealth
             .DistinctUntilChanged()
             .Subscribe(_playerView.UpdateHealth)
             .AddTo(_disposer);

            // Enemy collisions → damage player
            _playerView.EnemyCollided
                // If the enemy stays overlapping, throttle first to avoid damage every physics step
                .ThrottleFirst(k_HitCooldown)
                .Subscribe(enemyView =>
                {
                    // Prefer using EnemyStats.damage if available via the enemy's presenter/view.
                    // If EnemyView exposes a damage value later, read it here.
                    // For now, fall back to a safe default of 10 HP.
                    int damage = 10;

                    _playerModel.ApplyDamage(damage);
                })
                .AddTo(_disposer);

            // Player death hook (placeholder)
            _playerModel.Died
                .Subscribe(_ => UnityEngine.Debug.Log("[Player] Died"))
                .AddTo(_disposer);
        }
    }
}
