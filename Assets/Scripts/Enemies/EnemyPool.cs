using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class EnemyPool : MonoBehaviour
    {
        [Header("Prefab & Parents")]
        [SerializeField] private EnemyView m_enemyPrefab;
        [SerializeField] private Transform m_poolParent;   // assign in scene (Pooled)
        [SerializeField] private Transform m_activeParent; // optional

        private readonly Stack<EnemyView> m_inactive = new Stack<EnemyView>();

        private Transform GetParent(bool active)
        {
            if (active && m_activeParent != null) return m_activeParent;
            if (!active && m_poolParent != null) return m_poolParent;
            return this.transform;
        }

        public EnemyHandle Get(Transform playerTransform, EnemyStats stats)
        {
            EnemyView view = m_inactive.Count > 0 ? m_inactive.Pop() : Instantiate(m_enemyPrefab, GetParent(true));
            view.transform.SetParent(GetParent(true), false);
            view.gameObject.SetActive(true);

            var model = new EnemyModel();
            var presenter = new EnemyPresenter(model, view, playerTransform, stats);

            // Single-source pooling: only on ReturnedToPool
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
            Debug.Log($"[Pool] Returned: {view.name} -> {GetParent(false).name}");
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
