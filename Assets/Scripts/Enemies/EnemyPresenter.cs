using System;
using UniRx;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Presenter: binds Model ↔ View. Moves toward player when CanMove is true.
    /// Visibility transitions are event-based (OnBecameVisible/Invisible from view).
    /// </summary>
    public sealed class EnemyPresenter : IDisposable
    {
        private readonly IEnemyModel m_model;
        private readonly EnemyView m_view;
        private readonly Transform m_playerTransform;
        private readonly EnemyStats m_stats;
        private readonly CompositeDisposable m_disposables = new CompositeDisposable();

        public IObservable<Unit> ReturnedToPool => m_model.ReturnedToPool;

        public EnemyPresenter(IEnemyModel model, EnemyView view, Transform playerTransform, EnemyStats stats)
        {
            m_model = model;
            m_view = view;
            m_playerTransform = playerTransform;
            m_stats = stats;

            m_model.Initialize(m_stats);
            m_view.SetVisual(m_stats.sprite, m_stats.spriteScale);

            // Use event-based visibility to trigger transitions (no per-frame checks)
            m_view.VisibilityChanged
                .DistinctUntilChanged()
                .Subscribe(isVisible => m_model.SetOnScreen(isVisible))
                .AddTo(m_disposables);

            // Died → stop movement immediately
            m_model.Died.Subscribe(_ => m_view.Stop()).AddTo(m_disposables);

            // Per-frame sim + movement
            Observable.EveryUpdate()
                .Subscribe(_ => Tick())
                .AddTo(m_disposables);
        }

        public void Dispose() => m_disposables.Dispose();

        public void SpawnFromPool()
        {
            m_model.ResetForSpawn();
            m_view.SetActive(true);
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

            if (m_model.CanMove.Value && m_playerTransform != null)
            {
                Vector3 toPlayer = (m_playerTransform.position - m_view.Position);
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    Vector2 velocity = (Vector2)toPlayer.normalized * m_model.MoveSpeed;
                    m_view.ApplyVelocity(velocity);
                    return;
                }
            }

            m_view.ApplyVelocity(Vector2.zero);
        }
    }
}
