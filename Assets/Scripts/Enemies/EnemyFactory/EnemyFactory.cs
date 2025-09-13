using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly IObjectPool<EnemyView> m_pool;
        private readonly DiContainer m_container;

        // We resolve these on demand so concrete implementations can be swapped via DI.
        public EnemyFactory(IObjectPool<EnemyView> pool, DiContainer container)
        {
            m_pool = pool;
            m_container = container;
        }

        public IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            // 1) Rent a pooled view
            var view = m_pool.GetObject();
            view.transform.position = worldPosition;

            // 2) Create model & health via DI to respect your bindings
            var model = m_container.Instantiate<EnemyModel>();                 // IEnemyModel impl. :contentReference[oaicite:1]{index=1}
            var health = m_container.Resolve<IEnemyHealthModel>();             // bound elsewhere in your installers
            var playerView = m_container.Resolve<PlayerView>();                // your scene player view (if bound)
            var deathBus = m_container.Resolve<IEnemyDeathStream>();           // your death event stream (if bound)

            // 3) Create presenter; it initializes model and hooks up the view/UI. :contentReference[oaicite:2]{index=2}
            var presenter = m_container.Instantiate<EnemyPresenter>(
                new object[] { model, health, playerView, view, stats, deathBus });

            return new EnemyHandle(view, presenter, m_pool);
        }

        private sealed class EnemyHandle : IEnemyHandle
        {
            private readonly EnemyView m_view;
            private readonly EnemyPresenter m_presenter;
            private readonly IObjectPool<EnemyView> m_pool;

            public EnemyHandle(EnemyView view, EnemyPresenter presenter, IObjectPool<EnemyView> pool)
            {
                m_view = view;
                m_presenter = presenter;
                m_pool = pool;
            }

            public EnemyView View => m_view;
            public EnemyPresenter Presenter => m_presenter;

            public IObservable<Unit> ReturnedToPool => m_presenter.ReturnedToPool;

            public void Spawn()
            {
                // Presenter sets movement state, health UI, etc. :contentReference[oaicite:3]{index=3}
                m_presenter.SpawnFromPool();
            }

            public void Despawn()
            {
                m_presenter.DespawnToPool();
            }

            public void Release()
            {
                // Clean up reactive bindings then return the view to the pool.
                m_presenter.Dispose();
                m_pool.ReleaseObject(m_view);
            }
        }
    }
}
