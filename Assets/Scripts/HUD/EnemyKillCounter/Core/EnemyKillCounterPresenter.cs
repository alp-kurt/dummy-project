using System;
using UniRx;
using Zenject;

namespace Scripts
{
    public sealed class EnemyKillCounterPresenter : IInitializable, IDisposable
    {
        private readonly IEnemyKillCounterModel _model;
        private readonly EnemyKillCounterView _view;
        private readonly CompositeDisposable _cd = new CompositeDisposable();

        public EnemyKillCounterPresenter(IEnemyKillCounterModel model, EnemyKillCounterView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            // any enemy died -> +1
            EnemyPresenter.AnyDied
                .Subscribe(_ => _model.Increment())
                .AddTo(_cd);

            // render
            _model.Count
                  .Subscribe(_view.SetCount)
                  .AddTo(_cd);
        }

        public void Dispose() => _cd.Dispose();
    }
}
