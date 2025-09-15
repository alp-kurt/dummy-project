# Factory + Pooling Foundation

This package provides an extensible foundation for Factory and Object Pool patterns. It is designed as a base layer that you can build gameplay systems (enemies, projectiles, VFX, UI elements) on top of, with consideration for mobile performance.

## Features

* Factory Abstraction

    * IObjectFactory<T> defines how new objects are created.
    * PrefabFactory<T> creates Unity prefab instances safely, respecting parent transforms.

* Pooling Abstraction

    * IObjectPool<T> provides a minimal API for renting/releasing objects.
    * ObjectPool<T> implements a configurable pool with min/max capacity and prewarming.

* Poolable Contract

    * IPoolableObject defines a standard Unity-aware lifecycle: OnRent, OnRelease, and access to GameObject.
    * PooledView provides a base MonoBehaviour with default activation/deactivation logic.

* Lifecycle Hooks

    * Overrideable OnCreated, OnRented, and OnReleased in the pool for engine-specific behaviors.

* Safety

    * Enforces type safety at compile time (where T : Component, IPoolableObject).
    * Prevents runaway allocations via min/max limits.

    * Supports both GetObject() (throws if depleted) and TryGetObject(out T) (graceful fail).

* Prewarm
    * Instantiates a minimum pool upfront, avoiding spikes during gameplay.

***

## How To Use

1. Create a Poolable MonoBehaviour

        public sealed class EnemyView : PooledView
        {
            public override void OnRent()
            {
            base.OnRent();
            // Reset HP, play spawn anim, etc.
            }

            public override void OnRelease()
            {
            base.OnRelease();
            // Cleanup effects, reset state, etc.
            }
        }

2. Set Up a Factory

        [SerializeField] private EnemyView m_enemyPrefab;
        [SerializeField] private Transform m_parent;

        private IObjectFactory<EnemyView> _factory;

        void Awake()
        {
            _factory = new PrefabFactory<EnemyView>(m_enemyPrefab, m_parent);
        }

3. Set Up a Pool

        private IObjectPool<EnemyView> _pool;

        void Awake()
        {
            _pool = new ObjectPool<EnemyView>(_factory, min: 5, max: 50);
        }

4. Rent & Release

        void SpawnEnemy()
        {
            var enemy = _pool.GetObject();
            enemy.transform.position = GetSpawnPoint();
        }

        void DespawnEnemy(EnemyView enemy)
        {
            _pool.ReleaseObject(enemy);
        }

## Design Decisions

1. Compile-Time Safety
    * Both factory and pool constrain T to Component, IPoolableObject. This ensures:

    * Instantiations are valid Unity components.

    * Lifecycle methods (OnRent, OnRelease) are always available.

2. Interfaces First

    * IObjectFactory<T> / IObjectPool<T> allow swapping implementations or mocking for tests.

    * Keeps game logic decoupled from Unity-specific details.

3. Unity Coupling Is Explicit

    * PooledView centralizes Unity lifecycle logic (gameObject.SetActive).

    * Pool code doesn’t call GameObject directly except through the contract.

4. Simplicity Over Cleverness

    * No reflection, no DI container requirements.

    * Explicit PrefabFactory + ObjectPool setup keeps dependencies clear.