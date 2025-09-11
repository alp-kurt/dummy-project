using UniRx;
using UnityEngine;
using Zenject;

namespace Scripts
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] private JoystickView _joyStick;
        [SerializeField] private PlayerView _playerView;

        [SerializeField] private PlayerHealthView _playerHealthView;

        private readonly CompositeDisposable _disposer = new();

        private void OnDestroy() => _disposer.Dispose();

        public override void InstallBindings()
        {
            // Shared disposer
            Container.BindInterfacesAndSelfTo<CompositeDisposable>().FromInstance(_disposer).AsSingle();

            // Views
            Container.BindInstance(_playerView);
            Container.BindInstance(_joyStick);
            Container.BindInstance(_playerHealthView); 

            // Player Health MVP (standalone)
            Container.BindInterfacesTo<PlayerHealthModel>().AsSingle();
            Container.BindInterfacesTo<PlayerHealthPresenter>().AsSingle().NonLazy();

            // Player MVP (movement & state)
            Container.BindInterfacesTo<PlayerModel>().AsSingle();    
            Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();
        }
    }
}
