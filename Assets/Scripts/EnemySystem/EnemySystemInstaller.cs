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

        [Header("Spawner (optional override)")]
        [Tooltip("If left null, the view will be resolved via FromComponentInHierarchy().")]
        [SerializeField] private EnemyWaveSpawnerView _spawnerView;

        [Header("Wave Config (required)")]
        [SerializeField] private EnemyWaveConfig _waveConfig;

        public override void InstallBindings()
        {
            // ---- Validation ----
            if (_enemyPrefab == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Enemy prefab is not assigned.");
            if (_pooledParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Pooled parent is not assigned.");
            if (_activeParent == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: Active parent is not assigned.");
            if (_waveConfig == null) throw new System.Exception($"{nameof(EnemySystemInstaller)}: WaveConfig is not assigned.");
            if (_max < _min) throw new System.Exception($"{nameof(EnemySystemInstaller)}: max ({_max}) < min ({_min}).");

            // ---- Cross-cutting streams/services ----
            Container.Bind<IEnemyDeathStream>().To<EnemyDeathStream>().AsSingle();

            // ---- Scene singleton: ActiveEnemiesRoot (use activeParent) ----
            Container.Bind<Transform>()
                     .WithId("ActiveEnemiesRoot")
                     .FromInstance(_activeParent)
                     .AsSingle();

            // ---- Enemy view factory + pool ----
            Container.Bind<IObjectFactory<EnemyView>>()
                .To<PrefabFactory<EnemyView>>()
                .AsSingle()
                .WithArguments(_enemyPrefab, _pooledParent);

            Container.Bind<IObjectPool<EnemyView>>()
                .To<EnemyViewPool>()
                .AsSingle()
                .WithArguments(_min, _max, _pooledParent, _activeParent);

            Container.Bind<IEnemyViewRenter>().To<EnemyViewRenter>().AsSingle();
            Container.Bind<IEnemyPresenterFactory>().To<EnemyPresenterFactory>().AsSingle();
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();

            // ---- Spawner View ----
            if (_spawnerView)
                Container.Bind<EnemyWaveSpawnerView>().FromInstance(_spawnerView).AsSingle();
            else
                Container.Bind<EnemyWaveSpawnerView>().FromComponentInHierarchy().AsSingle();

            // ---- WaveConfig as a scene singleton ----
            Container.Bind<EnemyWaveConfig>().FromInstance(_waveConfig).AsSingle();

            // ---- Spawner Model: inject WaveConfig + RandomSeed (from view) ----
            var v = _spawnerView ? _spawnerView : Container.Resolve<EnemyWaveSpawnerView>();
            Container.Bind<IEnemyWaveSpawnerModel>()
                     .To<EnemyWaveSpawnerModel>()
                     .AsSingle()
                     .WithArguments(_waveConfig, v.RandomSeed);

            // ---- Spawner Presenter ----
            Container.BindInterfacesAndSelfTo<EnemyWaveSpawnerPresenter>().AsSingle().NonLazy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_min < 0) _min = 0;
            if (_max < 1) _max = 1;
            if (_max < _min)
            {
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] max ({_max}) < min ({_min}). Swapping.", this);
                (_min, _max) = (_max, _min);
            }

            if (!_enemyPrefab)
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Enemy prefab is not assigned.", this);

            if (!_pooledParent)
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Pooled parent is not assigned.", this);

            if (!_activeParent)
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] Active parent is not assigned.", this);

            if (_pooledParent && _activeParent && _pooledParent == _activeParent)
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] pooledParent and activeParent refer to the same Transform.", this);

            if (!_waveConfig)
                Debug.LogWarning($"[{nameof(EnemySystemInstaller)}] WaveConfig is not assigned.", this);

            if (!_spawnerView)
                Debug.Log($"[{nameof(EnemySystemInstaller)}] SpawnerView not set; will resolve via hierarchy.", this);
        }
#endif
    }
}
