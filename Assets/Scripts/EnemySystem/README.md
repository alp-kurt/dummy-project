# Enemy System

A lightweight enemy pipeline tuned for horde-style gameplay: pooled views, a presenter-driven mini state machine, and a wave spawner that paces creation work.

## Current Architecture

### Presenter-centric lifecycle
* **EnemyPresenter.cs** binds one `EnemyModel` to one `EnemyView` and tracks a private state enum: `Pooled → OutOfScreen → Active → Dead`. The presenter pushes the model/view into the right mode, despawns when the enemy stays off camera, and announces death via the static `AnyDied` stream.
* **EnemyModel.cs / IEnemyModel.cs** hold movement flags, health, and damage values. The presenter toggles `CanMove`, while the view forwards hits back to `ReceiveDamage`.
* **EnemyView.cs** is the pooled MonoBehaviour. It reports visibility changes, owns optional helpers like `EnemyHealthBarView`, and exposes `AttachModel` so the presenter can hook the model every spawn.

### Factory & pooling
* **EnemyFactory/** contains the workflow for spawning:
  * `EnemyFactory` rents a view, creates the model + presenter, then returns an `EnemyHandle` facade.
  * `EnemyHandle` exposes `Spawn`, `Despawn`, `ReturnedToPool`, and returns the view to the pool on `Release`.
  * `EnemyViewRenter` requests instances from the pool and does baseline transform reset.
  * `EnemyViewPool` inherits from the shared `ObjectPool` and handles parenting enemies under inactive vs. active roots.

### Spawning & configuration
* **EnemyWaveSpawner.cs** is a MonoBehaviour registered via `EnemySystemInstaller`. It reads an `EnemyWaveConfig`, optionally spreads work across frames, and asks the factory to create enemies just outside the camera view.
* **EnemyWaveConfig.cs** ScriptableObject describing which `EnemyStats` to spawn each wave, the interval between waves, and off-screen padding for spawn positions.
* **EnemyStats.cs** is the designer-facing asset for per-enemy health, movement speed, damage, sprite, and scale.
* **EnemyHealthBarView.cs** (optional) binds to the `EnemyView`'s model for UI feedback.

## Script Map
```
EnemySystem/
├── EnemyModel.cs
├── EnemyPresenter.cs
├── EnemyStats.cs
├── EnemyView.cs
├── EnemyWaveConfig.cs
├── EnemyWaveSpawner.cs
├── EnemySystemInstaller.cs
├── EnemyHealthBarView.cs
└── EnemyFactory/
    ├── EnemyFactory.cs
    ├── EnemyHandle.cs
    ├── EnemyViewPool.cs
    └── EnemyViewRenter.cs
```

## Setup & Usage

### Scene wiring
1. Add **EnemySystemInstaller** to a bootstrap GameObject.
2. Assign the fields exposed in the inspector:
   * **Enemy Prefab** – a prefab whose root has `EnemyView` (and optional helpers like `EnemyHealthBarView`).
   * **Pooled Parent** – inactive container Transform for rented-but-idle enemies.
   * **Active Parent** – Transform that holds spawned enemies.
   * **Min / Max** – pool capacity bounds used by `EnemyViewPool`.
3. Place an **EnemyWaveSpawner** component in the scene (the installer looks it up with `FromComponentInHierarchy`). Assign:
   * **Wave Config** (`EnemyWaveConfig` asset).
   * Optional pacing values: Spawn budget per frame, per-spawn delay, and random seed.
4. (Optional) Add a `PlayerView` so the presenter can chase the player; otherwise enemies simply move toward the origin.

### Assets & data
1. Create `EnemyStats` assets (`Create → Game → Enemies → Enemy Stats`) for each enemy type. Configure health, move speed, damage, sprite, and sprite scale.
2. Create one or more `EnemyWaveConfig` assets (`Create → Game → Waves → Multi-Enemy Wave Config`). Populate the Entries list with the `EnemyStats` assets and counts per wave, then tune the interval and off-screen padding.

### Runtime flow
* The wave spawner ticks `EnemyWaveConfig` entries, requests enemies from `EnemyFactory`, and positions them just outside the camera bounds.
* `EnemyFactory` rents a pooled view via `EnemyViewRenter`, builds a fresh model + presenter pair, and returns an `EnemyHandle` to the spawner.
* When the spawner calls `handle.Spawn()`, the presenter transitions to `OutOfScreen`. As soon as the `EnemyView` reports visibility, the state flips to `Active`; staying off-screen long enough returns to `Pooled`. Death also transitions to `Dead`, then to `Pooled` after the delay.
* Once the presenter signals `ReturnedToPool`, the spawner disposes the handle, returning the view to the pool.

### Layer & collision reminders
* Ensure enemies have the proper `Collider2D` and are on the correct physics layer for your project.
* Projectiles should call `IDamageable.ReceiveDamage` on collision, which the `EnemyView` forwards to its model.

This document reflects the scripts currently present under `Assets/Scripts/EnemySystem/` so designers and engineers can wire the feature without chasing removed modules.
