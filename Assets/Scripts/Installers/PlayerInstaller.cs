using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [Header("Config (ScriptableObject)")]
        [SerializeField] private PlayerHealthConfig healthConfig;

        [Header("Required scene references")]
        [SerializeField] private JoystickView joystickView;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private PlayerHealthView playerHealthView;

        public override void InstallBindings()
        {
            // Views — explicit, no auto-find
            Container.BindInstance(joystickView);
            Container.BindInstance(playerView);
            Container.BindInstance(playerHealthView);

            // Player core MVP
            Container.Bind<IPlayerModel>().To<PlayerModel>().AsSingle();
            Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();

            // Player health MVP (reads MaxHealth from SO)
            float maxHealth = healthConfig != null ? Mathf.Max(1f, healthConfig.MaxHealth) : 20f;

            Container.BindInterfacesAndSelfTo<PlayerHealthModel>()
                     .AsSingle()
                     .WithArguments(maxHealth);

            Container.BindInterfacesTo<PlayerHealthPresenter>().AsSingle().NonLazy();
        }
    }
}
