using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [Header("Config (ScriptableObject)")]
        [SerializeField] private PlayerHealthConfig healthConfig; // assign your asset in Inspector

        [Header("Auto-discover Views in Scene")]
        [Tooltip("If disabled, bind concrete instances manually with BindInstance(...) instead of FromComponentInHierarchy().")]
        [SerializeField] private bool autoDiscoverViews = true;

        // Optional manual references (only used when autoDiscoverViews = false)
        [SerializeField] private JoystickView joystickView;
        [SerializeField] private PlayerView playerView;
        [SerializeField] private PlayerHealthView playerHealthView;

        public override void InstallBindings()
        {
            // --- Views ---
            if (autoDiscoverViews)
            {
                Container.Bind<JoystickView>().FromComponentInHierarchy().AsSingle();
                Container.Bind<PlayerView>().FromComponentInHierarchy().AsSingle();
                Container.Bind<PlayerHealthView>().FromComponentInHierarchy().AsSingle();
            }
            else
            {
                Container.BindInstance(joystickView).IfNotBound();
                Container.BindInstance(playerView).IfNotBound();
                Container.BindInstance(playerHealthView).IfNotBound();
            }

            // --- Player core MVP ---
            Container.Bind<IPlayerModel>().To<PlayerModel>().AsSingle();
            Container.BindInterfacesTo<PlayerPresenter>().AsSingle().NonLazy();

            // --- Player health MVP (reads MaxHealth from SO) ---
            float maxHealth = healthConfig != null ? Mathf.Max(1f, healthConfig.MaxHealth) : 20f;
            Container.Bind<IPlayerHealthModel>()
                     .To<PlayerHealthModel>()
                     .AsSingle()
                     .WithArguments(maxHealth);

            Container.BindInterfacesTo<PlayerHealthPresenter>().AsSingle().NonLazy();
        }
    }
}
