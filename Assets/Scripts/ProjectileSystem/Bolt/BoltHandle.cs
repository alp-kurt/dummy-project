using System;          
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class BoltHandle : IBoltHandle, IDisposable
    {
        private readonly IBoltViewRenter m_renter;
        private readonly Subject<Unit> m_returnedSubject = new();
        private readonly CompositeDisposable m_disposables = new();
        private bool m_isDespawned;

        public BoltView View { get; }
        public BoltPresenter Presenter { get; }
        public IObservable<Unit> ReturnedToPool => m_returnedSubject;

        public BoltHandle(IBoltViewRenter renter, BoltView view, BoltPresenter presenter)
        {
            m_renter = renter;
            View = view;
            Presenter = presenter;

            Presenter.DespawnRequested
            .Subscribe(_ => Despawn())
            .AddTo(m_disposables);
        }

        public void Spawn(Vector3 position, Vector3 directionNormalized)
        {
            Presenter.InitializeMotion(position, directionNormalized);
        }

        public void Despawn()
        {
            if (m_isDespawned) return;
            m_isDespawned = true;

            Presenter.PrepareForDespawn();  // stops motion, deactivates view
            m_renter.Return(View);          // physically return to pool
            m_returnedSubject.OnNext(Unit.Default);
        }

        public void Release()
        {
            Despawn();
            Dispose();
        }

        public void Dispose()
        {
            m_disposables.Dispose();
            Presenter?.Dispose();
            m_returnedSubject.OnCompleted();
        }
    }
}
