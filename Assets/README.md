# Mobile Vampire Survivors Clone Prototype

A maintainable Unity prototype showcasing MVP (Model–View–Presenter), Zenject DI, Factory + Pooling foundation, and data-driven gameplay suitable for mobile LiveOps.

The project demonstrates separation of concerns, safe pooling, and designer-friendly tunables.

***

## Core Systems

* [**Factory + Pooling Foundation**](./Scripts/Global/FactoryBase/README.md) — Shared base layer for Enemies, Projectiles, VFX, UI. Includes:

    * IObjectFactory<T> / PrefabFactory<T> for instantiation.
    * IObjectPool<T> / ObjectPool<T> with prewarming, min/max capacity, safety guards.
    * IPoolableObject contract + PooledView base MonoBehaviour.
    * Compile-time safety (where T : Component, IPoolableObject).
    * Simplicity-first: no reflection, no DI requirement.

* [**Player System**](./Scripts/Player/README.md) — MVP with reactive health UI, collision filtered by physics layers. Health exposes normalized progress and death/damage streams.

* [**Enemy System**](./Scripts/EnemySystem/README.md) — High-volume, pooled enemies with FSM (Active / OutOfScreen / Dead / Pooled), wave spawner, and SO-driven stats (Minion/Champion). Motion in FixedUpdate via Rigidbody2D. Global death stream for HUD.

* [**Projectile System Foundation**](./Scripts/ProjectileSystem/README.md) — MVP base (Model, View, Presenter) with collision observables, despawn signaling, and config inheritance.

* [**Bolt Projectile**](./Scripts/ProjectileSystem/Bolt/README.md) — Concrete projectile: cone spawning, configurable lifetime & scale, ricochet logic, and Zenject-based spawner. Factory + pool included.

* [**Kill Counter HUD**](./Scripts/HUD/EnemyKillCounter/README.md) — Decoupled via IEnemyKillCounterView, updates a label through global IEnemyDeathStream.

## Getting Started

1. Open Unity (Unity 2022.3.47f1. Suggested)
2. Set up Layers: 
* Player, Enemy, Projectile. In Physics 2D → Collision Matrix enable:
    * Player ↔ Enemy
    * Projectile ↔ Enemy
    * Enemy ↔ Enemy
3. Load Assets/Scenes/Gameplay.unity
4. Add the following installers:
    * PlayerInstaller (Player MVP + Health).
    * EnemySystemInstaller (Enemy pool/factories/spawner).
    * BoltSystemInstaller (Bolt pool/factory/spawner).
    * Kill Counter Installer (HUD).
5. Configure ScriptableObjects 
    * EnemyStats
    * EnemyWaveConfig
    * BoltConfig

## Designer-Facing Configs

* EnemyStats: 
    * Max HP, 
    * Speed, 
    * Damage, 
    * Sprite, 
    * Scale.
    Used in WaveConfig entries.

* WaveConfig: 
    * List of enemy entries, 
    * Count per Wave, 
    * Interval Seconds, 
    * Offscreen Padding.

* ProjectileConfigBase: 
    * Display Name, 
    * Sprite, 
    * Base Damage/Speed.

* BoltConfig: Adds LifetimeSeconds, ScaleOverride.

* BoltSpawnerConfig: 
    * Bolts per cast, 
    * Seconds between casts, 
    * optional debug logging.

* Kill Counter View — Label format (e.g., “Kills: {0}”).

## Architecture Notes

* MVP per feature — Models hold logic, Views are thin MonoBehaviours, Presenters manage bindings.

* Zenject DI — Dependencies injected; no FindObjectOfType.

* Factory + Pooling — All entities created via factories and pooled. Supports min/max limits, prewarming, and safe rent/release cycles.

* FSM (Enemies) — Simple states keep lifecycle explicit (Active, OutOfScreen, Dead, Pooled).

* Reactive Streams — Health, deaths, collisions exposed as observables.

## LiveOps & Performance

* Data-driven tuning — Values adjustable via ScriptableObjects.

* Pooling-first — Enemy and Projectile systems built on shared pool foundation.

* GC awareness — No per-frame allocations in Update loops; pooled disposables cleared on despawn.

* Observability — Global kill stream; an example of an extensible system for telemetry and analytics.

## **Authors Note:**

This prototype was built with a focus on clarity, scalability, and mobile LiveOps readiness. A few key design choices guided the implementation:

### Design Choices

1. **Team-friendliness**: Validators and multiple standalone README files ensure that each system (Enemies, Projectiles, Factory, HUD, etc.) can be understood and adjusted independently by designers and developers.

2. **Designer-driven tuning:** Core parameters are exposed through ScriptableObjects, making balance adjustments fast while also enabling future remote config integrations.

3. **Performance awareness:** Memory is managed through a Factory + Pooling foundation, which reduces GC allocations and avoids runtime instantiations. Prefab-based setup minimizes scene updates, and redundant collision checks are eliminated through a layer-based collision matrix.

4. **Code quality:** Adopted naming the private fields '_camelCase' for naming consistency and readability. The architecture emphasizes SOLID principles, favoring modular, maintainable systems.

5. **Enemy system depth:** Enemies use a lightweight FSM (Active, OutOfScreen, Dead, etc.), designed to support future states (e.g., a Frozen State similar to [Heroes vs Hordes'](https://play.google.com/store/apps/details?id=com.swiftgames.survival&hl=en) after revive effect).

6. **Event-driven telemetry:** An abstract event bus tracks enemy kills, powering the kill counter HUD and laying the groundwork for other counters or Firebase analytics integrations.

7. **Projectile extensibility:** A Projectile base defines shared behaviors, enabling new projectile types with unique logic. Introduced Damage and Speed classes with upgrade functions, preparing the system for **perk-based** power-ups.

8. **Player feedback:** Enemies trigger on-hit effects, improving the game’s responsiveness and overall user feel.

---
Together, these decisions result in a scalable, maintainable foundation for evolving a mobile game into a LiveOps-ready product.

## Author

[Alp Kurt](https://alpkurt.com) | krtalp@gmail.com