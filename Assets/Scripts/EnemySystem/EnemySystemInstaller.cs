using UnityEngine;
using Zenject;

namespace Scripts
{
    public sealed class EnemySystemInstaller : MonoInstaller
    {
        [Header("Enemy View")]
        [SerializeField] private EnemyView _enemyPrefab;
        [SerializeField] private Transform _pooledParent;
        [SerializeField] private Transform _activeParent;

        [Header("Pool")]
        [SerializeField, Min(0)] private int _min = 16;
        [SerializeField, Min(1)] private int _max = 128;

        public override void InstallBindings()
        {
            // ---- Validation ----
            if (_enemyPrefab == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (_pooledParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (_activeParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");
            if (_max < _min) throw new System.Exception($"{nameof(EnemySystemInstaller)}: max ({_max}) < min ({_min}).");

            if (!Container.HasBinding<SignalBus>())
            {
                SignalBusInstaller.Install(Container);
            }

            Container.DeclareSignal<EnemySpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<EnemyDiedSignal>().OptionalSubscriber();
            Container.DeclareSignal<EnemyReturnedToPoolSignal>().OptionalSubscriber();

            Container.Bind<Transform>()
                     .WithId("PooledEnemiesRoot")
                     .FromInstance(_pooledParent)
                     .AsCached();

            // ---- Active root for presenters/factories ----
            Container.Bind<Transform>()
                     .WithId("ActiveEnemiesRoot")
                     .FromInstance(_activeParent)
                     .AsCached();

            // ---- Enemy view memory pool ----
            Container.BindMemoryPool<EnemyView, EnemyViewPool>()
                     .WithInitialSize(_min)
                     .WithMaxSize(_max)
                     .FromComponentInNewPrefab(_enemyPrefab)
                     .UnderTransform(_pooledParent);
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            // ---- Simple spawner (MonoBehaviour in scene; no MVP) ----
            Container.BindInterfacesAndSelfTo<EnemyWaveSpawner>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_min < 0) _min = 0;
            if (_max < 1) _max = 1;
            if (_max < _min) { (_min, _max) = (_max, _min); }

            if (!_enemyPrefab) Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Enemy prefab is not assigned.", this);
            if (!_pooledParent) Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Pooled parent is not assigned.", this);
            if (!_activeParent) Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Active parent is not assigned.", this);
        }
#endif
    }
}
