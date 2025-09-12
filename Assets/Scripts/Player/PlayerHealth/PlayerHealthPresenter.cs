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
            // Bind health to UI; auto-disposed with the view’s lifecycle
            _model.CurrentHealth01
                  .DistinctUntilChanged()
                  .Subscribe(_view.SetHealth01)
                  .AddTo(_view);

            // Optional: hook damage/heal for quick effects/logs
            _model.Damaged
                  .Subscribe(_ => { /* e.g., hit flash, SFX */ })
                  .AddTo(_view);

            _model.Healed
                  .Subscribe(_ => { /* e.g., heal sparkle, SFX */ })
                  .AddTo(_view);

            // Single-fire death hook
            _model.Died
                  .Subscribe(_ => UnityEngine.Debug.Log("[Player] Died"))
                  .AddTo(_view);
        }
    }
}
