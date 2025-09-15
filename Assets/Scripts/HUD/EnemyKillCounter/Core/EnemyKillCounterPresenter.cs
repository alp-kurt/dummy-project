using System;
using UniRx;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Wires enemy death events to the kill counter model and binds the model to the view.
    /// Creates exactly two subscriptions and disposes them when the presenter is disposed.
    /// </summary>
    public sealed class EnemyKillCounterPresenter : IInitializable, IDisposable
    {
        private readonly IEnemyDeathStream _deaths;
        private readonly IEnemyKillCounterModel _model;
        private readonly EnemyKillCounterView _view;
        private readonly CompositeDisposable _cd = new CompositeDisposable();

        /// <summary>
        /// All dependencies are injected; no runtime lookups.
        /// </summary>
        [Inject]
        public EnemyKillCounterPresenter(IEnemyDeathStream deaths,
                                         IEnemyKillCounterModel model,
                                         EnemyKillCounterView view)
        {
            _deaths = deaths;
            _model = model;
            _view = view;
        }

        /// <summary>
        /// Subscribes to:
        /// 1) enemy death stream -> increment model
        /// 2) model.Count -> update view
        /// </summary>
        public void Initialize()
        {
            _deaths.Died.Subscribe(_ => _model.Increment()).AddTo(_cd);
            _model.Count.Subscribe(_view.SetCount).AddTo(_cd);
        }

        /// <summary>
        /// Disposes subscriptions created in <see cref="Initialize"/>.
        /// Call when the context (scene/UI) is torn down.
        /// </summary>
        public void Dispose() => _cd.Dispose();
    }
}
