using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [Header("Config")]
        [SerializeField, Min(1f)] private float playerMaxHealth = 20f;
        [SerializeField, Min(0f)] private float playerMoveSpeed = 2f;

        [Header("Required scene references")]
        [SerializeField] private JoystickView joystickView;
        [SerializeField] private PlayerHealthView playerHealthView;

        public override void InstallBindings()
        {
            // Views
            Container.BindInstance(joystickView);
            Container.BindInstance(playerHealthView);

            Container.Bind<PlayerView>()
                    .FromComponentInHierarchy()
                    .AsSingle();

            // Player core MVP (inject speed)
            Container.Bind<IPlayerModel>()
                     .To<PlayerModel>()
                     .AsSingle()
                     .WithArguments(Mathf.Max(0f, playerMoveSpeed));

            Container.BindInterfacesTo<PlayerPresenter>()
                    .AsSingle()
                    .NonLazy();

            // Player health MVP
            Container.BindInterfacesAndSelfTo<PlayerHealthModel>()
                     .AsSingle()
                     .WithArguments(Mathf.Max(1f, playerMaxHealth));

            Container.BindInterfacesTo<PlayerHealthPresenter>()
                     .AsSingle()
                     .NonLazy();
        }
    }
}
