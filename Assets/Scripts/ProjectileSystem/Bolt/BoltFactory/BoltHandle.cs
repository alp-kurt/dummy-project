using System;          
using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class BoltHandle : IBoltHandle, IDisposable
    {
        private readonly BoltViewPool _pool;
        private readonly SignalBus _signalBus;
        private readonly Subject<Unit> _returnedSubject = new();
        private readonly CompositeDisposable _disposables = new();
        private bool _isDespawned;

        public BoltView View { get; }
        public BoltPresenter Presenter { get; }
        public IObservable<Unit> ReturnedToPool => _returnedSubject;

        public BoltHandle(BoltViewPool pool, BoltView view, BoltPresenter presenter, SignalBus signalBus)
        {
            _pool = pool;
            View = view;
            Presenter = presenter;
            _signalBus = signalBus;

            Presenter.DespawnRequested
            .Subscribe(_ => Despawn())
            .AddTo(_disposables);

            ReturnedToPool
                .Take(1)
                .Subscribe(_ => Dispose())
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
            _pool.Despawn(View);           // physically return to pool
            _returnedSubject.OnNext(Unit.Default);

            _signalBus.Fire(new BoltReturnedToPoolSignal { View = View });
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
