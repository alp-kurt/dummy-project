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
        private readonly EnemyViewPool _viewPool;
        private readonly IInstantiator _instantiator;
        private readonly PlayerView _player;

        public EnemyFactory(
            EnemyViewPool viewPool,
            IInstantiator instantiator,
            [Inject(Optional = true)] PlayerView player
        )
        {
            _viewPool = viewPool;
            _instantiator = instantiator;
            _player = player;
        }

        public IEnemyHandle Create(EnemyStats stats, Vector3 worldPosition)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            var view = _viewPool.Spawn();
            view.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
            view.transform.localScale = Vector3.one;
            var model = _instantiator.Instantiate<EnemyModel>();
            var presenter = _instantiator.Instantiate<EnemyPresenter>(
                new object[] { model, view, stats, _player ? _player.transform : null });

            return new EnemyHandle(view, presenter, _viewPool);
        }
    }
}
