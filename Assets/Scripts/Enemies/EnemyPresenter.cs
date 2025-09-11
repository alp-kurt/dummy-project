using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPresenter : IDisposable
    {
        private readonly IEnemyModel m_model;
        private readonly EnemyView m_view;
        private readonly Transform m_player;
        private readonly EnemyStats m_stats;
        private readonly CompositeDisposable m_disposables = new CompositeDisposable();

        [InjectOptional] private IEnemyUiService m_ui;
        [InjectOptional] private IEnemySfxService m_sfx;
        [InjectOptional] private IEnemyAnalyticsService m_analytics;

        [Inject]
        public EnemyPresenter(
            IEnemyModel model,
            PlayerView playerView,
            EnemyView view,
            EnemyStats stats)
        {
            m_model = model;
            m_view = view;
            m_stats = stats;
            m_player = playerView != null ? playerView.transform : null;

            m_model.Initialize(m_stats);
            m_view.SetVisual(m_stats.sprite, m_stats.spriteScale);

            // Health -> UI (normalized)
            m_model.Health
                .CombineLatest(m_model.MaxHealth, (h, max) => max > 0 ? (float)h / max : 0f)
                .DistinctUntilChanged()
                .Subscribe(m_view.UpdateHealth)
                .AddTo(m_disposables);

            // Visibility-driven activation
            m_view.VisibilityChanged
                .DistinctUntilChanged()
                .Subscribe(m_model.SetOnScreen)
                .AddTo(m_disposables);

            // Typed events (UI/SFX/Analytics hooks)
            m_model.DamagedTyped
                .Subscribe(ev =>
                {
                    var pos = m_view.Position;
                    m_ui?.OnDamaged(ev, pos);
                    m_sfx?.OnDamaged(ev, pos);
                    m_analytics?.OnDamaged(ev);
                })
                .AddTo(m_disposables);

            m_model.DiedTyped
                .Subscribe(ev =>
                {
                    m_view.Stop();
                    var pos = m_view.Position;
                    m_ui?.OnDied(ev, pos);
                    m_sfx?.OnDied(ev, pos);
                    m_analytics?.OnDied(ev);
                })
                .AddTo(m_disposables);

            m_model.ReturnedToPoolTyped
                .Subscribe(ev =>
                {
                    var pos = m_view.Position;
                    m_ui?.OnReturned(ev, pos);
                    m_sfx?.OnReturned(ev, pos);
                    m_analytics?.OnReturned(ev);
                })
                .AddTo(m_disposables);

            // Per-frame tick + movement
            Observable.EveryUpdate()
                .Subscribe(_ => Tick())
                .AddTo(m_disposables);
        }

        // Pool reclaim signal (unchanged)
        public IObservable<Unit> ReturnedToPool => m_model.ReturnedToPool;

        public void Dispose() => m_disposables.Dispose();

        public void SpawnFromPool()
        {
            m_model.ResetForSpawn();
            m_view.SetActive(true);
            m_view.UpdateHealth(1f);         // show full on spawn
            m_view.SetHealthVisible(true);   // ensure visible when taking damage soon
        }

        public void DespawnToPool()
        {
            m_view.Stop();
            m_view.SetActive(false);
        }

        private void Tick()
        {
            float dt = Time.deltaTime;
            m_model.Tick(dt);

            if (m_player != null && m_model.CanMove.Value)
            {
                var dir = (m_player.position - m_view.Position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    var vel = (Vector2)dir.normalized * m_model.MoveSpeed;
                    m_view.ApplyVelocity(vel);
                    return;
                }
            }

            m_view.ApplyVelocity(Vector2.zero);
        }
    }
}
