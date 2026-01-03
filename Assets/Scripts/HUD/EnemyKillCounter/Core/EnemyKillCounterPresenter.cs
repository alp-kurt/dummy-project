using System;
using UniRx;
using Zenject;

namespace Scripts
{
    public sealed class EnemyKillCounterPresenter : IInitializable, IDisposable
    {
        private readonly IEnemyKillCounterModel _model;
        private readonly EnemyKillCounterView _view;
        private readonly SignalBus _signalBus;
        private readonly CompositeDisposable _cd = new CompositeDisposable();

        public EnemyKillCounterPresenter(IEnemyKillCounterModel model, EnemyKillCounterView view, SignalBus signalBus)
        {
            _model = model;
            _view = view;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<EnemyDiedSignal>(OnEnemyDied);

            // render
            _model.Count
                  .Subscribe(_view.SetCount)
                  .AddTo(_cd);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemyDiedSignal>(OnEnemyDied);
            _cd.Dispose();
        }

        private void OnEnemyDied(EnemyDiedSignal signal)
        {
            _model.Increment();
        }
    }
}
