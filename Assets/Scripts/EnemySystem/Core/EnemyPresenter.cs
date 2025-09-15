using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private readonly IEnemyModel _model;
        private readonly IEnemyHealthModel _health;
        private readonly EnemyView _view;
        private readonly Transform _player;
        private readonly EnemyStats _stats;
        private readonly IEnemyDeathStream _deathBus;

        private readonly CompositeDisposable _disposables = new();

        // Modular view parts (optional per prefab)
        private EnemyHitFxView _hitFx;
        private EnemyHealthBarView _healthBar;

        [Inject]
        public EnemyPresenter(
            IEnemyModel model,
            IEnemyHealthModel health,
            PlayerView playerView,
            EnemyView view,
            EnemyStats stats,
            IEnemyDeathStream deathBus)
        {
            _model = model;
            _health = health;
            _view = view;
            _stats = stats;
            _player = playerView != null ? playerView.transform : null;
            _deathBus = deathBus;

            // ----- Initialize model & visuals -----
            _health.Initialize(_stats.maxHealth);
            _model.Initialize(_stats);
            _model.SetHealth(_health);

            _view.SetVisual(_stats.sprite, _stats.spriteScale);
            _view.SetContactDamage(_stats.damage);

            // Cache optional modules from the view/prefab
            _view.EnsureModulesCached();
            _hitFx = _view.HitFxView;
            _healthBar = _view.HealthBarView;

            // ----- Bind streams -----

            // Health → UI
            _health.CurrentHealth01
                   .DistinctUntilChanged()
                   .Subscribe(v => _healthBar?.SetHealth01(v))
                   .AddTo(_disposables);

            // Damage → On-hit FX
            _health.Damaged
                   .Subscribe(_ => _hitFx?.PlayOnHit())
                   .AddTo(_disposables);

            // Visibility → movement state
            _view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(_model.SetOnScreen)
                 .AddTo(_disposables);

            // Death → bus + stop
            _health.Died
                   .Subscribe(_ =>
                   {
                       _deathBus.Publish();
                       _view.Stop();
                   })
                   .AddTo(_disposables);

            // Physics-safe driving
            Observable.EveryFixedUpdate()
                      .Subscribe(_ => TickFixed(Time.fixedDeltaTime))
                      .AddTo(_disposables);
        }

        /// <summary>Forward the model's pooling signal to spawners.</summary>
        public IObservable<Unit> ReturnedToPool => _model.ReturnedToPool;

        public void Dispose() => _disposables.Dispose();

        public void SpawnFromPool()
        {
            _model.ResetForSpawn();

            // Keep contact damage in sync with stats at spawn
            _view.SetContactDamage(_stats.damage);

            _view.SetActive(true);

            // Reset modular views for a fresh lifetime
            _healthBar?.OnSpawn();
            _hitFx?.OnSpawn();
            _healthBar?.SetVisible(true);
        }

        public void DespawnToPool()
        {
            // Stop any motion first
            _view.Stop();

            // Clean modular views (kill tweens, reset visuals)
            _hitFx?.OnDespawn();
            _healthBar?.OnDespawn();

            _view.SetActive(false);
        }

        private void TickFixed(float fixedDt)
        {
            _model.Tick(fixedDt);

            if (_player != null && _model.CanMove.Value)
            {
                var dir = (_player.position - _view.Position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    var vel = (Vector2)dir.normalized * _model.MoveSpeed;
                    _view.ApplyVelocityFixed(vel, fixedDt);
                    return;
                }
            }

            _view.ApplyVelocityFixed(Vector2.zero, fixedDt);
        }
    }
}
