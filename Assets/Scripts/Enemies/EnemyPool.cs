using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemyPool : MonoBehaviour
    {
        [Header("Prefab & Parents (required)")]
        [SerializeField] private EnemyView enemyPrefab;
        [SerializeField] private Transform activeParent;
        [SerializeField] private Transform pooledParent;

        [Header("Performance")]
        [SerializeField, Min(0)] private int prewarmCount = 18;

        private readonly Stack<EnemyView> m_inactive = new Stack<EnemyView>();

        [Inject] private GameFactory _factory;

        private void Awake()
        {
            if (!enemyPrefab) throw new NullReferenceException($"{nameof(EnemyPool)}: Enemy prefab is not assigned.");
            if (!activeParent) throw new NullReferenceException($"{nameof(EnemyPool)}: Active parent is not assigned.");
            if (!pooledParent) throw new NullReferenceException($"{nameof(EnemyPool)}: Pooled parent is not assigned.");

            Prewarm(prewarmCount);   
        }

        /// <summary>
        /// Gets an enemy view, wires its presenter, and returns the view for positioning.
        /// </summary>
        public EnemyView Get(EnemyStats stats)
        {
            var view = m_inactive.Count > 0
                ? m_inactive.Pop()
                : Instantiate(enemyPrefab, activeParent);

            if (view.transform.parent != activeParent)
                view.transform.SetParent(activeParent, false);

            view.gameObject.SetActive(true);

            // Build presenter with DI; it will publish ReturnedToPool when done
            var presenter = _factory.CreateEnemy(view, stats);

            presenter.ReturnedToPool
                .Take(1)
                .Subscribe(_ => Return(view, presenter))
                .AddTo(view); // tie to view lifetime (safe with pooling)

            presenter.SpawnFromPool();
            return view;
        }

        private void Return(EnemyView view, EnemyPresenter presenter)
        {
            presenter.Dispose();

            view.Stop();
            view.gameObject.SetActive(false);
            view.transform.SetParent(pooledParent, false);

            m_inactive.Push(view);
        }

        // Populates the pool at setup phase to reduce startup lag spikes
        private void Prewarm(int i)
        {
            for (i = 0; i < prewarmCount; i++)
            {
                var v = Instantiate(enemyPrefab, pooledParent);
                v.gameObject.SetActive(false);
                m_inactive.Push(v);
            }
        }
    }
}
