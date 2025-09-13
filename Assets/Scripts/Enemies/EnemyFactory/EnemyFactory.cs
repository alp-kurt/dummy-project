using System;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Orchestrates: rent view - create presenter glue - return a handle.
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyViewRenter m_viewRenter;
        private readonly IEnemyPresenterFactory m_presenterFactory;

        public EnemyFactory(IEnemyViewRenter viewRenter, IEnemyPresenterFactory presenterFactory)
        {
            m_viewRenter = viewRenter;
            m_presenterFactory = presenterFactory;
        }

        public IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            var view = m_viewRenter.Rent(worldPosition);
            var presenter = m_presenterFactory.Create(view, stats);

            return new EnemyHandle(view, presenter, m_viewRenter);
        }
    }
}
