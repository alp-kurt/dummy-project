using UniRx;
using Zenject;

namespace Scripts
{
    public sealed class PlayerHealthPresenter : IInitializable
    {
        private readonly PlayerHealthView _view;
        private readonly IPlayerHealthModel _model;

        public PlayerHealthPresenter(PlayerHealthView view, IPlayerHealthModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _model.CurrentHealth01
                  .DistinctUntilChanged()
                  .Subscribe(_view.SetHealth01)
                  .AddTo(_view);

            _model.Damaged
                  .Subscribe(_ => { /* placeholder for effect logic */ })
                  .AddTo(_view);

            _model.Healed
                  .Subscribe(_ => { /* placeholder for effect logic */ })
                  .AddTo(_view);

            _model.Died
                  .Subscribe(_ => UnityEngine.Debug.Log("[Player] Died"))
                  .AddTo(_view);
        }
    }
}
