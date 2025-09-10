using UniRx;
using Zenject;
using UniRx;

namespace Scripts
{
    public class PlayerPresenter : IInitializable
    {
        private readonly JoystickView _joystickView;
        private readonly PlayerView _playerView;
        private readonly IPlayerModel _playerModel;
        private readonly CompositeDisposable _disposer;

        public PlayerPresenter(JoystickView joystickView, PlayerView playerView, IPlayerModel playerModel,
            CompositeDisposable disposer)
        {
            _joystickView = joystickView;
            _playerView = playerView;
            _playerModel = playerModel;
            _disposer = disposer;
        }

        public void Initialize()
        {
            _joystickView.OnInput.Subscribe(_playerView.Move).AddTo(_disposer);
            
            _playerModel.CurrentHealth.Subscribe(_playerView.UpdateHealth).AddTo(_disposer);
        }
    }
}