using System;
using UniRx;
using Zenject;

namespace Scripts
{
    public sealed class EnemyKillCounterPresenter : IInitializable, IDisposable
    {
        private readonly IEnemyDeathStream _deaths;
        private readonly IEnemyKillCounterModel _model;
        private readonly EnemyKillCounterView _view;
        private readonly CompositeDisposable _cd = new CompositeDisposable();

        [Inject]
        public EnemyKillCounterPresenter(IEnemyDeathStream deaths,
                                         IEnemyKillCounterModel model,
                                         EnemyKillCounterView view)
        {
            _deaths = deaths; _model = model; _view = view;
        }

        public void Initialize()
        {
            _deaths.Died.Subscribe(_ => _model.Increment()).AddTo(_cd);
            _model.Count.Subscribe(_view.SetCount).AddTo(_cd);
        }

        public void Dispose() => _cd.Dispose();
    }
}