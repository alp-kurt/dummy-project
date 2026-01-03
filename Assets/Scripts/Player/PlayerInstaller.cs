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

        [Header("Camera (optional override)")]
        [Tooltip("If left null, Camera will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private Camera _camera;

        public override void InstallBindings()
        {
            // ---- Validation ----
            if (!_joystickView) throw new System.Exception($"{nameof(PlayerInstaller)}: JoystickView is not assigned.");
            if (_playerMaxHealth < 1f || _playerMoveSpeed < 0f)
                Debug.LogWarning($"[{nameof(PlayerInstaller)}] Clamping invalid config: MaxHealth={_playerMaxHealth}, MoveSpeed={_playerMoveSpeed}", this);

            if (!Container.HasBinding<SignalBus>())
            {
                SignalBusInstaller.Install(Container);
            }

            Container.DeclareSignal<PlayerDiedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerEnemyCollidedSignal>().OptionalSubscriber();

            // Views
            Container.BindInstance(_joystickView);

            Container.Bind<PlayerView>()
                .FromComponentInHierarchy()
                .AsSingle();

            // Player core (speed + max health)
            Container.Bind<IPlayerModel>()
                .To<PlayerModel>()
                .AsSingle()
                .WithArguments(Mathf.Max(0f, _playerMoveSpeed), Mathf.Max(1f, _playerMaxHealth));

            Container.BindInterfacesTo<PlayerPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<PlayerHealthBarView>()
                .FromComponentInHierarchy()
                .AsSingle();

            // Camera
            if (_camera)
            {
                Container.Bind<Camera>()
                    .FromInstance(_camera)
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

            if (!_camera)
                Debug.Log($"[{nameof(PlayerInstaller)}] Camera not set; will resolve via hierarchy.", this);
        }
#endif
    }
}
