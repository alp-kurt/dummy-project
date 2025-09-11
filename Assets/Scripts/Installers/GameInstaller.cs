using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] private JoystickView _joyStick;
        [SerializeField] private PlayerView _playerView;

        private readonly CompositeDisposable _disposer = new();
        
        private void OnDestroy()
        {
            _disposer.Dispose();
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CompositeDisposable>().AsSingle();

            Container.BindInstance(_playerView);
            Container.BindInstance(_joyStick);

            Container.BindInterfacesTo<PlayerModel>().AsSingle();
            Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();
        }
    }
}