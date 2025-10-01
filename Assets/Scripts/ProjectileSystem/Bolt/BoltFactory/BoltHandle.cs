using System;          
using UniRx;
using UnityEngine;

namespace Scripts
{
    public sealed class BoltHandle : IBoltHandle, IDisposable
    {
        private readonly IBoltViewRenter _renter;
        private readonly Subject<Unit> _returnedSubject = new();
        private readonly CompositeDisposable _disposables = new();
        private bool _isDespawned;

        public BoltView View { get; }
        public BoltPresenter Presenter { get; }
        public IObservable<Unit> ReturnedToPool => _returnedSubject;

        public BoltHandle(IBoltViewRenter renter, BoltView view, BoltPresenter presenter)
        {
            _renter = renter;
            View = view;
            Presenter = presenter;

            Presenter.DespawnRequested
            .Subscribe(_ => Despawn())
            .AddTo(_disposables);
        }

        public void Spawn(Vector3 position, Vector3 directionNormalized)
        {
            Presenter.InitializeMotion(position, directionNormalized);
        }

        public void Despawn()
        {
            if (_isDespawned) return;
            _isDespawned = true;

            Presenter.PrepareForDespawn();  // stops motion, deactivates view
            _renter.Return(View);          // physically return to pool
            _returnedSubject.OnNext(Unit.Default);
        }

        public void Release()
        {
            Despawn();
            Dispose();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            Presenter?.Dispose();
            _returnedSubject.OnCompleted();
        }
    }
}
