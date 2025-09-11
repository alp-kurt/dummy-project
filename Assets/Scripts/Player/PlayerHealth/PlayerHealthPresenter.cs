using UniRx;
using Zenject;

namespace Scripts
{
    /// <summary>
    /// Presenter that binds PlayerHealthModel to PlayerHealthView.
    /// </summary>
    public sealed class PlayerHealthPresenter : IInitializable
    {
        private readonly PlayerHealthView _view;
        private readonly IPlayerHealthModel _model;
        private readonly CompositeDisposable _disposer;

        public PlayerHealthPresenter(PlayerHealthView view,
                                     IPlayerHealthModel model,
                                     CompositeDisposable disposer)
        {
            _view = view;
            _model = model;
            _disposer = disposer;
        }

        public void Initialize()
        {
            _model.CurrentHealth01
                .DistinctUntilChanged()
                .Subscribe(_view.SetHealth01)
                .AddTo(_disposer);

            _model.Died
                .Subscribe(_ => UnityEngine.Debug.Log("[Player] Died"))
                .AddTo(_disposer);
        }
    }
}
