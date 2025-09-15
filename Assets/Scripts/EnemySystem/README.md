# Enemy System

A modular, Enemy System built with pooling, and a lightweight FSM. It’s tuned for Heroes-vs-Hordes–style gameplay: lots of enemies, off-screen spawning, pooling, and quick iteration for LiveOps.

## Features

* High-volume friendly: Pooling for enemy views (min/max capacity).

* Per-frame spawn budget or per-spawn delay to avoid frame spikes.

* Clean separation

* FSM for lifecycle (on-screen, off-screen, dead, pooled): a foundation for future states like frozen.

* LiveOps-tunable, Stats via ScriptableObjects (EnemyStats, MinionStats, ChampionStats).

* Wave parameters via WaveConfig on the spawner view.

* On-hit feedback: squash with DOTween, killed on despawn (pool-safe).

* Physics-safe motion: Movement in FixedUpdate via Rigidbody2D.MovePosition.

* Global EnemyDeathStream via Kill Counter HUD

## Design Decisions

1. Hybrid DI: Root container for globals, per-enemy instances via Instantiate<T>. Unity components on the prefab are initialized manually in the presenter factory.

2. Singleton states: FSM states are singletons; per-enemy timers live in EnemyContext, preventing cross-enemy interference.

3. View modules: Tween-heavy visual bits live in small view modules (e.g., EnemyHitFxView, EnemyHealthBarView), keeping EnemyView lean and pool-safe.


## Script Structure

### Enemies/

* EnemySystemInstaller.cs      // unified installer (pool, factories, spawner, kill counter)

### Enemies/Core

* EnemyModel.cs                // gameplay state, FSM gateway
* EnemyView.cs                 // render/motion shell (lean)
* EnemyPresenter.cs            // wires model↔view, physics tick, subscriptions
* EnemyStats.cs                // base SO (MinionStats.cs, ChampionStats.cs)
* EnemyHitFxView.cs            // squash; pool-safe

### Enemies/EnemyHealth/

* EnemyHealthModel.cs          // health, Damaged/Died, normalized health
* EnemyHealthBarView.cs        // slider + fade; pool-safe
* EnemyDamageableAdapter.cs    // bridges collision → health.ReceiveDamage

### Enemies/EnemyStates/
    
* EnemyStateMachine.cs
* EnemyStateBase.cs
* EnemyState_Active.cs         
* EnemyState_OutOfScreen.cs    // singleton (+ offscreen timer via context CTS)
* EnemyState_Dead.cs           // singleton (+ death timer via context CTS)
* EnemyState_Pooled.cs         
* EnemyContext.cs              // per-enemy

### Enemies/EnemyFactory/
    
* EnemyPresenterFactory.cs     // per-enemy instances; adapter init; ctor DI for globals
* EnemyFactory.cs              // rents view → builds presenter/model/health → handle
* EnemyViewPool.cs             // min/max pool; parent mgmt
* EnemyViewRenter.cs           // rent/return façade (+ transform reset)

### Enemies/EnemySpawner/

* EnemyWaveSpawnerView.cs      // wave config + pacing tunables
* EnemyWaveSpawnerModel.cs     // wave state (index, interval, padding)
* EnemyWaveSpawnerPresenter.cs // async loop, TryCreate, pacing

### Enemies/EnemyEvents

* EnemyDeathStream.cs          // death stream on enemy kills

## Setup & Usage

### Scene Setup

1. Ensure a PlayerView exists in the scene (bound FromComponentInHierarchy).

2. Add EnemySystemInstaller to a bootstrap GameObject and assign:

3. enemyPrefab (your EnemyView prefab)

4. pooledParent (inactive pool parent)

5. activeParent (active enemies parent)

6. min/max pool sizes

7. Add/assign EnemyWaveSpawnerView in the scene:

8. Set WaveConfig (entries, counts, interval, off-screen padding).

### Create Stats

1. Make MinionStats / ChampionStats assets (menu: Create → …).

2. Reference these assets in your WaveConfig entries.

### Layers/Collision

1. Create Enemy, Player, Projectile Layers and set the accordingly

2. Make sure the following structure:
    * Player applies to Enemy and Enemy 
    * Enemy applies to both Player and Projectile
    * Projectile applies to Enemy

### Configuration & Tunables

* EnemyStats SO

* WaveConfig (on Spawner View)

* Spawner view extras: 

* FSM Timing (per enemy)

    1. In EnemyContext: OffscreenDespawnSeconds, DeathDespawnSeconds
    2. UseUnscaledTime

***

## EnemyStats (Per-Enemy Base Stats)

The core stats and visuals for an enemy type (used by wave entries).

* Create: Use your concrete enemy stat assets (e.g., MinionStats, ChampionStats) from your game’s Enemies menu. If you don’t see them, ask a programmer to expose concrete assets that inherit from EnemyStats.

* Used where: In WaveConfig → Entries → Stats. You don’t put stats on the enemy prefab; the wave controls what spawns.

### Fields (Designer Guide)

* Max Health
    * Total HP. Minimum 1 (auto-clamped).

* Movement Speed
    * Units per second. 0 = stationary. Can be fractional.

* Damage
    * Contact damage dealt to the player. Minimum 1 (auto-clamped).

* Sprite
    * The enemy’s sprite. The asset warns if this is missing.

* Sprite Scale
    * Visual scale multiplier at spawn (≥ 0.1). Use to make variants look bigger/smaller without changing gameplay.

The asset clamps bad values and warns when Sprite is missing.

---
## WaveConfig (Enemy Waves)

List of enemies (per type) to spawn per wave, plus timing and spawn-area padding.

* Create: Assets → Create → Game → Waves → Multi-Enemy Wave Config

### Assign in Scene: 

Find your EnemySystemInstaller in the SceneContext and drop your WaveConfig into its Wave Config field. The spawner reads this via DI; you don’t add WaveConfig anywhere else.

### Fields (Designer Guide)

* Entries
    * A list of items the spawner will produce each wave, top to bottom.

* Stats: 
    * Pick an EnemyStats asset for that enemy type.

* Count Per Wave: 
    * How many of that enemy to spawn each wave. 0 = skip this entry (useful for testing).

* Wave Interval Seconds
    * Time between consecutive waves. Minimum 0.05s (the asset clamps values below this).

* Offscreen Padding
    * How far outside the camera edge enemies appear. Larger values spawn further offscreen (more travel time before they enter view).

The asset warns if an Entry is missing Stats or if counts are invalid.
Spawn pacing per frame is not set here—adjust that on the EnemyWaveSpawnerView in the scene (Spawn Budget per Frame / Spawn Delay Seconds).