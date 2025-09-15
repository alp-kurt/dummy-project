using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private readonly IEnemyModel m_model;
        private readonly IEnemyHealthModel m_health;
        private readonly EnemyView m_view;
        private readonly Transform m_player;
        private readonly EnemyStats m_stats;
        private readonly IEnemyDeathStream m_deathBus;

        private readonly CompositeDisposable m_disposables = new();

        // Modular view parts (optional per prefab)
        private EnemyHitFxView m_hitFx;
        private EnemyHealthBarView m_healthBar;

        [Inject]
        public EnemyPresenter(
            IEnemyModel model,
            IEnemyHealthModel health,
            PlayerView playerView,
            EnemyView view,
            EnemyStats stats,
            IEnemyDeathStream deathBus)
        {
            m_model = model;
            m_health = health;
            m_view = view;
            m_stats = stats;
            m_player = playerView != null ? playerView.transform : null;
            m_deathBus = deathBus;

            // ----- Initialize model & visuals -----
            m_health.Initialize(m_stats.maxHealth);
            m_model.Initialize(m_stats);
            m_model.SetHealth(m_health);

            m_view.SetVisual(m_stats.sprite, m_stats.spriteScale);
            m_view.SetContactDamage(m_stats.damage);

            // Cache optional modules from the view/prefab
            m_view.EnsureModulesCached();
            m_hitFx = m_view.HitFxView;
            m_healthBar = m_view.HealthBarView;

            // ----- Bind streams -----

            // Health → UI
            m_health.CurrentHealth01
                   .DistinctUntilChanged()
                   .Subscribe(v => m_healthBar?.SetHealth01(v))
                   .AddTo(m_disposables);

            // Damage → On-hit FX
            m_health.Damaged
                   .Subscribe(_ => m_hitFx?.PlayOnHit())
                   .AddTo(m_disposables);

            // Visibility → movement state
            m_view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(m_model.SetOnScreen)
                 .AddTo(m_disposables);

            // Death → bus + stop
            m_health.Died
                   .Subscribe(_ =>
                   {
                       m_deathBus.Publish();
                       m_view.Stop();
                   })
                   .AddTo(m_disposables);

            // Physics-safe driving
            Observable.EveryFixedUpdate()
                      .Subscribe(_ => TickFixed(Time.fixedDeltaTime))
                      .AddTo(m_disposables);
        }

        /// <summary>Forward the model's pooling signal to spawners.</summary>
        public IObservable<Unit> ReturnedToPool => m_model.ReturnedToPool;

        public void Dispose() => m_disposables.Dispose();

        public void SpawnFromPool()
        {
            m_model.ResetForSpawn();

            // Keep contact damage in sync with stats at spawn
            m_view.SetContactDamage(m_stats.damage);

            m_view.SetActive(true);

            // Reset modular views for a fresh lifetime
            m_healthBar?.OnSpawn();
            m_hitFx?.OnSpawn();
            m_healthBar?.SetVisible(true);
        }

        public void DespawnToPool()
        {
            // Stop any motion first
            m_view.Stop();

            // Clean modular views (kill tweens, reset visuals)
            m_hitFx?.OnDespawn();
            m_healthBar?.OnDespawn();

            m_view.SetActive(false);
        }

        private void TickFixed(float fixedDt)
        {
            m_model.Tick(fixedDt);

            if (m_player != null && m_model.CanMove.Value)
            {
                var dir = (m_player.position - m_view.Position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    var vel = (Vector2)dir.normalized * m_model.MoveSpeed;
                    m_view.ApplyVelocityFixed(vel, fixedDt);
                    return;
                }
            }

            m_view.ApplyVelocityFixed(Vector2.zero, fixedDt);
        }
    }
}
