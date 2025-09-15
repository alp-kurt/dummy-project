using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class PlayerInstaller : MonoInstaller
    {
        [Header("Config")]
        [SerializeField, Min(1f)] private float _playerMaxHealth = 20f;
        [SerializeField, Min(0f)] private float _playerMoveSpeed = 2f;

        [Header("Required scene references")]
        [SerializeField] private JoystickView _joystickView;
        [SerializeField] private PlayerHealthView _playerHealthView;

        [Header("Camera (optional override)")]
        [Tooltip("If left null, Camera will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private Camera camera;

        public override void InstallBindings()
        {
            // ---- Validation ----
            if (!_joystickView) throw new System.Exception($"{nameof(PlayerInstaller)}: JoystickView is not assigned.");
            if (!_playerHealthView) throw new System.Exception($"{nameof(PlayerInstaller)}: PlayerHealthView is not assigned.");
            if (_playerMaxHealth < 1f || _playerMoveSpeed < 0f)
                Debug.LogWarning($"[{nameof(PlayerInstaller)}] Clamping invalid config: MaxHealth={_playerMaxHealth}, MoveSpeed={_playerMoveSpeed}", this);

            // Views
            Container.BindInstance(_joystickView);
            Container.BindInstance(_playerHealthView);

            Container.Bind<PlayerView>()
                    .FromComponentInHierarchy()
                    .AsSingle();

            Container.Bind<IPlayerPosition>()
                    .To<PlayerPositionAdapter>()
                    .AsSingle();

            // Player core MVP (inject speed)
            Container.Bind<IPlayerModel>()
                     .To<PlayerModel>()
                     .AsSingle()
                     .WithArguments(Mathf.Max(0f, _playerMoveSpeed));

            Container.BindInterfacesTo<PlayerPresenter>()
                    .AsSingle()
                    .NonLazy();

            // Player health MVP
            Container.BindInterfacesAndSelfTo<PlayerHealthModel>()
                     .AsSingle()
                     .WithArguments(Mathf.Max(1f, _playerMaxHealth));

            Container.BindInterfacesTo<PlayerHealthPresenter>()
                     .AsSingle()
                     .NonLazy();

            // Camera (prefer explicit instance, else fallback to hierarchy)
            if (camera)
            {
                Container.Bind<Camera>()
                         .FromInstance(camera)
                         .AsSingle()
                         .IfNotBound();
            }
            else
            {
                Container.Bind<Camera>()
                         .FromComponentInHierarchy()
                         .AsSingle()
                         .IfNotBound();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_playerMaxHealth < 1f) _playerMaxHealth = 1f;
            if (_playerMoveSpeed < 0f) _playerMoveSpeed = 0f;

            if (!_joystickView)
                Debug.LogWarning($"[{nameof(PlayerInstaller)}] JoystickView is not assigned.", this);

            if (!_playerHealthView)
                Debug.LogWarning($"[{nameof(PlayerInstaller)}] PlayerHealthView is not assigned.", this);

            if (!camera)
                Debug.Log($"[{nameof(PlayerInstaller)}] Camera not set; will resolve via hierarchy.", this);
        }
#endif
    }
}
