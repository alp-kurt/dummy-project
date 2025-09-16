using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Wires Enemy model/health to view and player; owns FixedUpdate driving of movement
    /// and forwards lifecycle events (death → bus, pooling signals, etc.).
    /// </summary>
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

        /// <summary>
        /// DI entry point. Initializes model & health, applies visuals, and binds reactive streams.
        /// Player is optional; if missing, movement falls back to zero velocity.
        /// </summary>
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
            _health.Initialize(_stats.MaxHealth);
            _model.Initialize(_stats);
            _model.SetHealth(_health);

            _view.SetVisual(_stats.Sprite, _stats.SpriteScale);
            _view.SetContactDamage(_stats.Damage);

            // Cache optional modules from the view/prefab
            _view.EnsureModulesCached();
            _hitFx = _view.HitFxView;
            _healthBar = _view.HealthBarView;

            // ----- Bind streams -----

            // Health -> UI bar (only when value actually changes).
            _health.CurrentHealth01
                   .DistinctUntilChanged()
                   .Subscribe(v => _healthBar?.SetHealth01(v))
                   .AddTo(_disposables);

            // Health damage -> On-hit FX (squash/stretch, etc.).
            _health.Damaged
                   .Subscribe(_ => _hitFx?.PlayOnHit())
                   .AddTo(_disposables);

            // Renderer visibility -> model screen-state (drives FSM transitions).
            _view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(_model.SetOnScreen)
                 .AddTo(_disposables);

            // Death -> publish to bus and ensure motion stops.
            _health.Died
                   .Subscribe(_ =>
                   {
                       _deathBus.Publish();
                       _view.Stop();
                   })
                   .AddTo(_disposables);

            // Physics-safe driving: move in FixedUpdate cadence.
            Observable.EveryFixedUpdate()
                      .Subscribe(_ => TickFixed(Time.fixedDeltaTime))
                      .AddTo(_disposables);
        }

        /// <summary>Forward the model's pooling signal to spawners (e.g., factories/pools).</summary>
        public IObservable<Unit> ReturnedToPool => _model.ReturnedToPool;

        public void Dispose() => _disposables.Dispose();

        /// <summary>
        /// Called when the enemy is taken from the pool for a fresh lifetime.
        /// Resets model state, re-applies contact damage from stats, and refreshes modular views.
        /// </summary>
        public void SpawnFromPool()
        {
            _model.ResetForSpawn();

            // Keep contact damage in sync with stats at spawn.
            _view.SetContactDamage(_stats.Damage);

            _view.SetActive(true);

            // Reset modular views for a fresh lifetime.
            _healthBar?.OnSpawn();
            _hitFx?.OnSpawn();
            _healthBar?.SetVisible(true);
        }

        /// <summary>
        /// Called before returning to the pool. Stops motion and cleans up modular views
        /// (kills tweens, resets UI), then deactivates the GameObject.
        /// </summary>
        public void DespawnToPool()
        {
            // Stop any motion first.
            _view.Stop();

            // Clean modular views (kill tweens, reset visuals).
            _hitFx?.OnDespawn();
            _healthBar?.OnDespawn();

            _view.SetActive(false);
        }

        /// <summary>
        /// Fixed-step movement driver:
        /// - Ticks model FSM with fixedDt.
        /// - If player exists and model.CanMove is true, push velocity toward the player at MoveSpeed.
        /// - Otherwise, apply zero velocity (idle).
        /// </summary>
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
