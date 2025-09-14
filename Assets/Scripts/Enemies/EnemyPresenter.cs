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
        private readonly CompositeDisposable m_disposables = new();
        private readonly IEnemyDeathStream m_deathBus;

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

            // Init slices
            m_health.Initialize(m_stats.maxHealth);
            m_model.Initialize(m_stats);
            m_model.SetHealth(m_health);

            m_view.SetVisual(m_stats.sprite, m_stats.spriteScale);
            m_view.SetContactDamage(m_stats.damage);


            // Health → UI
            m_health.CurrentHealth01
                   .DistinctUntilChanged()
                   .Subscribe(m_view.UpdateHealth)
                   .AddTo(m_disposables);

            // Visibility → movement state
            m_view.VisibilityChanged
                 .DistinctUntilChanged()
                 .Subscribe(m_model.SetOnScreen)
                 .AddTo(m_disposables);

            // Death → bus + view stop
            m_health.Died
                   .Subscribe(_ =>
                   {
                       m_deathBus.Publish();
                       m_view.Stop();
                   })
                   .AddTo(m_disposables);

            // Tick movement
            Observable.EveryUpdate()
                      .Subscribe(_ => Tick())
                      .AddTo(m_disposables);
        }

        public IObservable<Unit> ReturnedToPool => m_model.ReturnedToPool;
        public void Dispose() => m_disposables.Dispose();

        public void SpawnFromPool()
        {
            m_model.ResetForSpawn();
            m_view.SetContactDamage(m_stats.damage);
            m_view.SetActive(true);
            m_view.UpdateHealth(1f);
            m_view.SetHealthVisible(true);
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
