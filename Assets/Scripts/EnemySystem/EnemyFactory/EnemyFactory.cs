using System;
using UnityEngine;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Orchestrates: rent view - create presenter glue - return a handle.
    /// </summary>
    public sealed class EnemyFactory : IEnemyFactory
    {
        private readonly IEnemyViewRenter _viewRenter;
        private readonly PlayerView _player;

        public EnemyFactory(IEnemyViewRenter viewRenter, [Inject(Optional = true)] PlayerView player)
        {
            _viewRenter = viewRenter;
            _player = player;
        }

        public IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            var view = _viewRenter.Rent(worldPosition);
            var model = new EnemyModel();
            var presenter = new EnemyPresenter(model, view, stats, _player ? _player.transform : null);

            return new EnemyHandle(view, presenter, _viewRenter);
        }
    }
}
