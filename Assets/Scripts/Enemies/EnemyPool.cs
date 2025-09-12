using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPool : MonoBehaviour
    {
        [Header("Prefab & Parents")]
        [SerializeField] private EnemyView m_enemyPrefab;

        // Children under this object named exactly "Active" and "Pooled"
        [SerializeField] private Transform m_activeParent;
        [SerializeField] private Transform m_pooledParent;

        private readonly Stack<EnemyView> m_inactive = new Stack<EnemyView>();

        [Inject] private EnemyPresenterFactory m_presenterFactory;

        private void Awake()
        {
            // Auto-find children named "Active" and "Pooled" if not assigned
            if (m_activeParent == null) m_activeParent = transform.Find("Active");
            if (m_pooledParent == null) m_pooledParent = transform.Find("Pooled");

            if (m_activeParent == null) m_activeParent = this.transform;
            if (m_pooledParent == null) m_pooledParent = this.transform;
        }

        private Transform GetParent(bool active) => active ? m_activeParent : m_pooledParent;

        public EnemyHandle Get(EnemyStats stats)
        {
            var view = m_inactive.Count > 0
                ? m_inactive.Pop()
                : Instantiate(m_enemyPrefab, GetParent(true));

            view.transform.SetParent(GetParent(true), false);
            view.gameObject.SetActive(true);

            // Presenter via Zenject factory (player is injected inside presenter)
            var presenter = m_presenterFactory.Create(view, stats);

            var sub = presenter.ReturnedToPool
                .Take(1)
                .Subscribe(_ => Return(view, presenter, sub: null));

            presenter.SpawnFromPool();
            return new EnemyHandle(view, presenter, sub);
        }

        private void Return(EnemyView view, EnemyPresenter presenter, IDisposable sub)
        {
            presenter.Dispose();
            sub?.Dispose();

            view.Stop();
            view.gameObject.SetActive(false);
            view.transform.SetParent(GetParent(false), true);

            m_inactive.Push(view);
        }

        public readonly struct EnemyHandle
        {
            public readonly EnemyView View;
            public readonly EnemyPresenter Presenter;
            public readonly IDisposable Subscription;

            public EnemyHandle(EnemyView v, EnemyPresenter p, IDisposable sub)
            {
                View = v; Presenter = p; Subscription = sub;
            }
        }
    }
}
