# Bolt | Projectile Type

The Bolt System is a concrete projectile built on top of the shared Projectile System Foundation (MVP, pooling, factory, reactive flows).

 It adds ricochet-ready bolts, lifetime, configurable visuals (scale override), and a pure-DI spawner that fires from the player at configurable intervals. 
 
 See the [foundation doc](.././README.md) for the base architecture, shared APIs, and design rationales (MVP split, pooling, reactive collisions, despawn signaling).

 ***

 ## Architecture (MVP + Pool + Factory)

* Model (per bolt): BoltModel.cs — lifetime and stats derived from BoltConfig. It aligns with the foundation’s ProjectileModel responsibilities (state, activate/deactivate).

* Presenter (per bolt): BoltPresenter.cs — moves the bolt and orchestrates collisions and despawn. Mirrors the foundation’s Presenter responsibilities (Initialize / per-spawn InitializeMotion / Despawn).

* Targeting: BoltTargetingService.cs — resolves redirect directions (closest visible enemy or a safe random fallback).
    * Uses EnemyRegistry (signal-driven) for fast, allocation-free lookups.

* View (pooled prefab): BoltView (your prefab/component) — sprite/collider and pooling hooks as described by the foundation’s ProjectileView.

* Pooling: BoltViewPool.cs — Zenject `MonoMemoryPool` handling parenting and activation/reset.

* Factory: BoltFactory.cs + BoltPresenterFactory.cs — composes View+Model+Presenter into a BoltHandle and applies scale override immediately after rent to keep visuals consistent.

* Spawner (system level MVP):

    * Config: BoltSpawnerConfig.cs (bolts per cast, seconds between casts, optional logging)

    * Model: BoltSpawnerModel.cs (timing heartbeat; reads SO live to allow in-play tuning)

    * Presenter: BoltSpawnerPresenter.cs (ticks via Zenject, spawns using IBoltFactory from IPlayerPosition)

## Setup & Usage

Use BoltSystemInstaller.cs in your SceneContext; it binds:

* Pooling & Factory: BoltViewPool (MonoMemoryPool), IBoltPresenterFactory, IBoltFactory

* Ricochet Tunables: edge padding, cooldown (ids)

* Spawner (pure DI): BoltSpawnerConfig + BoltConfig, IBoltSpawnerModel, BoltSpawnerPresenter (as ITickable)

* Player Position: ensure PlayerInstaller binds IPlayerPosition (adapter over PlayerView)

* Signals: BoltSpawnedSignal, BoltReturnedToPoolSignal

## Designer Tuning

* BoltConfig (Assets/Create/Game/Projectiles/Bolt)

    * Base Damage
    * Base Speed
    * Sprite
    * Display Name: inherited from ProjectileConfigBase.
    * LifetimeSeconds: 0 → infinite.
    * ScaleOverride (optional): > 0 forces a uniform localScale at spawn; 0 keeps prefab scale.

* BoltSpawnerConfig (Assets/Create/Configs/Projectiles/Bolt Spawner Config)

    * boltsPerCast: how many bolts per interval.
    * secondsBetweenCasts: cadence between casts.
    * logCasts: optional debug output.
