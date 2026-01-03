using UniRx;
using System;

namespace Scripts
{
    /// <summary>
    /// Owns one created enemy’s lifetime and returns it to the pool on Release().
    /// </summary>
    public sealed class EnemyHandle : IEnemyHandle
    {
        private readonly EnemyView _view;
        private readonly EnemyPresenter _presenter;
        private readonly EnemyViewPool _pool;
        private readonly CompositeDisposable _disposables = new();

        public EnemyHandle(EnemyView view, EnemyPresenter presenter, EnemyViewPool pool)
        {
            _view = view;
            _presenter = presenter;
            _pool = pool;

            _presenter.ReturnedToPool
                      .Take(1)
                      .Subscribe(_ => Release())
                      .AddTo(_disposables);
        }

        public EnemyView View => _view;
        public EnemyPresenter Presenter => _presenter;

        public IObservable<Unit> ReturnedToPool => _presenter.ReturnedToPool;

        public void Spawn() => _presenter.SpawnFromPool();
        public void Despawn() => _presenter.DespawnToPool();

        public void Release()
        {
            Despawn();
            _presenter.Dispose();
            _pool.Despawn(_view);
            _disposables.Dispose();
        }
    }
}
