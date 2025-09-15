using System;
using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Orchestrates: rent view - create presenter glue - return a handle.
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyViewRenter _viewRenter;
        private readonly IEnemyPresenterFactory _presenterFactory;

        public EnemyFactory(IEnemyViewRenter viewRenter, IEnemyPresenterFactory presenterFactory)
        {
            _viewRenter = viewRenter;
            _presenterFactory = presenterFactory;
        }

        public IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            var view = _viewRenter.Rent(worldPosition);
            var presenter = _presenterFactory.Create(view, stats);

            return new EnemyHandle(view, presenter, _viewRenter);
        }
    }
}
