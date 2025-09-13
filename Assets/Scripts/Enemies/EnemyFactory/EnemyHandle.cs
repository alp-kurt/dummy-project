using UniRx;
using System;

namespace Scripts
{
    /// <summary>
    /// Owns one created enemy’s lifetime and returns it to the pool on Release().
    /// </summary>
    public sealed class EnemyHandle : IEnemyHandle
    {
        private readonly EnemyView m_view;
        private readonly EnemyPresenter m_presenter;
        private readonly IEnemyViewRenter m_renter;

        public EnemyHandle(EnemyView view, EnemyPresenter presenter, IEnemyViewRenter renter)
        {
            m_view = view;
            m_presenter = presenter;
            m_renter = renter;
        }

        public EnemyView View => m_view;
        public EnemyPresenter Presenter => m_presenter;

        public IObservable<Unit> ReturnedToPool => m_presenter.ReturnedToPool;

        public void Spawn() => m_presenter.SpawnFromPool();
        public void Despawn() => m_presenter.DespawnToPool();

        public void Release()
        {
            m_presenter.Dispose();
            m_renter.Return(m_view);
        }
    }
}
